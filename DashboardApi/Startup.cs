/*
 * Dashboard API startup.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using SensateService.ApiCore.Middleware;
using SensateService.Config;
using SensateService.Infrastructure.Sql;
using SensateService.Init;
using SensateService.Models;

namespace SensateService.DashboardApi
{
	public class Startup
	{
		private readonly IWebHostEnvironment _env;
		private readonly IConfiguration _configuration;

		public Startup(IConfiguration configuration, IWebHostEnvironment environment)
		{
			this._configuration = configuration;
			this._env = environment;
		}

		// ReSharper disable once UnusedMember.Global
		public void ConfigureServices(IServiceCollection services)
		{
			var cache = new CacheConfig();
			var db = new DatabaseConfig();
			var auth = new AuthenticationConfig();

			this._configuration.GetSection("Cache").Bind(cache);
			this._configuration.GetSection("Authentication").Bind(auth);
			this._configuration.GetSection("Database").Bind(db);

			services.AddCors();

			services.AddPostgres(db.PgSQL.ConnectionString);
			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);

			services.Configure<UserAccountSettings>(options => {
				options.JwtKey = auth.JwtKey;
				options.JwtIssuer = auth.JwtIssuer;
				options.JwtExpireMinutes = auth.JwtExpireMinutes;
				options.JwtRefreshExpireMinutes = auth.JwtRefreshExpireMinutes;
				options.PublicUrl = auth.PublicUrl;
				options.Scheme = auth.Scheme;
			});

			/*
			 * Setup user authentication
			 */
			services.AddIdentity<SensateUser, SensateRole>(config => {
				config.SignIn.RequireConfirmedEmail = true;
			})
			.AddEntityFrameworkStores<SensateSqlContext>()
			.AddDefaultTokenProviders();

			JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

			services.Configure<IdentityOptions>(options => {
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireUppercase = true;
				options.Password.RequiredLength = 8;
				options.Password.RequiredUniqueChars = 5;

				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.AllowedForNewUsers = true;

				options.User.RequireUniqueEmail = true;
			});

			services.AddAuthentication(options => {
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(cfg => {
				cfg.RequireHttpsMetadata = false;
				cfg.SaveToken = true;
				cfg.TokenValidationParameters = new TokenValidationParameters
				{
					ValidIssuer = auth.JwtIssuer,
					ValidAudience = auth.JwtIssuer,
					IssuerSigningKey = new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes(auth.JwtKey)
					),
					ClockSkew = TimeSpan.Zero
				};
			});

			if(cache.Enabled)
                services.AddCacheStrategy(cache, db);

			/* Add repositories */
			services.AddSqlRepositories(cache.Enabled);
			services.AddDocumentRepositories(cache.Enabled);

			services.AddSwaggerGen(c => {
				c.SwaggerDoc("v1", new OpenApiInfo {
					Title = "Sensate Dashboard API - Version 1",
					Version = "v1" 
				});
			});

			services.AddRouting();
			services.AddControllers().AddNewtonsoftJson();

			services.AddLogging((logging) => {
				logging.AddConsole();
				if(this._env.IsDevelopment())
					logging.AddDebug();
			});
		}

		// ReSharper disable once UnusedMember.Global
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider sp)
		{
			var auth = new AuthenticationConfig();
			this._configuration.GetSection("Authentication").Bind(auth);

			app.UseRouting();

			app.UseCors(p => {
				p.WithOrigins("http://localhost:4200", auth.JwtIssuer)
				.AllowAnyHeader()
				.AllowAnyMethod()
				.AllowCredentials();
			});

			using(var scope = sp.CreateScope()) {
				var ctx = scope.ServiceProvider.GetRequiredService<SensateSqlContext>();
				ctx.Database.EnsureCreated();
				ctx.Database.Migrate();
			}

			app.UseSwagger();
			app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sensate Dashboard API v1"); });

			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseAuthentication();
			app.UseAuthorization();
			app.UseMiddleware<RequestLoggingMiddleware>();
			app.UseEndpoints(ep => { ep.MapControllers(); });
		}
	}
}
