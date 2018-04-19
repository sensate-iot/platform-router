/*
 * .NET core services startup.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Versioning;

using Swashbuckle.AspNetCore.Swagger;

using SensateService.Models;
using SensateService.Infrastructure.Sql;
using SensateService.Init;
using SensateService.Services;
using SensateService.Middleware;

namespace SensateService
{
	public class Startup
	{
		public Startup(IConfiguration configuration, IHostingEnvironment environment)
		{
			var builder = new ConfigurationBuilder()
							.SetBasePath(environment.ContentRootPath)
							.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
							.AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true);

			if(environment.IsDevelopment()) {
				builder.AddUserSecrets<Startup>();
				builder.AddApplicationInsightsSettings(developerMode: true);
			}

			builder.AddEnvironmentVariables();
			this.Secrets = builder.Build();
			this.Configuration = configuration;
		}

		public IConfiguration Configuration { get; }
		public IConfiguration Secrets {get;}

		// ReSharper disable once UnusedMember.Global
		public void ConfigureServices(IServiceCollection services)
		{
			string pgsql;

			pgsql = this.Secrets.GetValue<string>("PgSqlConnectionString");
			services.AddCors();

			services.AddPostgres(pgsql);
			services.AddDocumentStore(Secrets["MongoDbConnectionString"], Secrets["MongoDbDatabaseName"]);

			services.Configure<UserAccountSettings>(options => {
				options.JwtKey = this.Secrets["JwtKey"];
				options.JwtIssuer = this.Secrets["JwtIssuer"];
				options.JwtExpireMinutes = Int32.Parse(this.Secrets["JwtExpireMinutes"]);
				options.JwtRefreshExpireMinutes = Int32.Parse(this.Secrets["JwtRefreshExpireMinutes"]);
				options.ConfirmForward = this.Configuration["ConfirmForward"];
			});

			services.AddApiVersioning(options => {
				options.ApiVersionReader = new QueryStringApiVersionReader();
				options.AssumeDefaultVersionWhenUnspecified = true;
				options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
			});

			/*
			 * Setup user authentication
			 */
			services.AddIdentity<SensateUser, UserRole>(config => {
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
					ValidIssuer = Secrets["JwtIssuer"],
					ValidAudience = Secrets["JwtIssuer"],
					IssuerSigningKey = new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes(Secrets["JwtKey"])
					),
					ClockSkew = TimeSpan.Zero
				};
			});

			services.AddDistributedRedisCache(opts => {
				opts.Configuration = Configuration["RedisHost"];
				opts.InstanceName = Configuration["RedisInstanceName"];
			});

			/* Add repositories */
			services.AddSqlRepositories();
			services.AddCacheStrategy(Configuration["CacheType"]);
			services.AddDocumentRepositories(Configuration["Cache"] == "true");

			if(Configuration["EmailProvider"] == "SendGrid") {
				services.AddSingleton<IEmailSender, SendGridMailer>();
				services.Configure<SendGridAuthOptions>(opts => {
					opts.FromName = Configuration["EmailFromName"];
					opts.From = Configuration["EmailFrom"];
					opts.Key = Secrets["SendGridKey"];
					opts.Username = Secrets["SendGridUser"];
				});
			} else if(Configuration["EmailProvider"] == "SMTP") {
				services.AddSingleton<IEmailSender, SmtpMailer>();
				services.Configure<SmtpAuthOptions>(opts => {
					opts.FromName = Configuration["EmailFromName"];
					opts.From = Configuration["EmailFrom"];
					opts.Password = Secrets["SmtpPassword"];
					opts.Username = Secrets["SmtpUsername"];
					opts.Ssl = Configuration["SmtpSsl"] == "true";
					opts.Port = Int16.Parse(Configuration["SmtpPort"]);
					opts.Host = Configuration["SmtpHost"];
				});
			}

			services.AddSwaggerGen(c => {
				c.SwaggerDoc("v1", new Info {
					Title = "Sensate API - Version 1",
					Version = "v1"
				});
			});

			services.AddWebSocketService();
			services.AddMvc();
		}

		// ReSharper disable once UnusedMember.Global
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider sp)
		{
			app.UseCors(p => {
				p.AllowAnyOrigin()
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
			app.UseSwaggerUI(c => {
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sensate API - Version 1");
			});

			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseWebSockets();
			app.MapWebSocketService("/measurement", sp.GetService<WebSocketMeasurementHandler>());
			app.UseAuthentication();
			app.UseMvc();
		}
	}
}
