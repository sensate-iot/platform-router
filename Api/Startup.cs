/*
 * .NET core services startup.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using SensateService.Api.Middleware;
using SensateService.Config;
using SensateService.Infrastructure.Sql;
using SensateService.Init;
using SensateService.Models;
using SensateService.Services;
using Swashbuckle.AspNetCore.Swagger;

namespace SensateService.Api
{
	public class Startup
	{
		public Startup(IConfiguration configuration, IHostingEnvironment environment)
		{
			this._configuration = configuration;
		}

		private readonly IConfiguration _configuration;

		// ReSharper disable once UnusedMember.Global
		public void ConfigureServices(IServiceCollection services)
		{
			var cache = new CacheConfig();
			var db = new DatabaseConfig();
			var auth = new AuthenticationConfig();
			var mail = new MailConfig();
			var text = new TextConfig();

			this._configuration.GetSection("Cache").Bind(cache);
			this._configuration.GetSection("Authentication").Bind(auth);
			this._configuration.GetSection("Database").Bind(db);
			this._configuration.GetSection("Mail").Bind(mail);
			this._configuration.GetSection("Text").Bind(text);

			services.AddCors();

			services.AddPostgres(db.PgSQL.ConnectionString);
			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName);

			//services.AddLogging(builder => { builder.AddConfiguration(this._configuration.GetSection("Logging"));

			services.Configure<UserAccountSettings>(options => {
				options.JwtKey = auth.JwtKey;
				options.JwtIssuer = auth.JwtIssuer;
				options.JwtExpireMinutes = auth.JwtExpireMinutes;
				options.JwtRefreshExpireMinutes = auth.JwtRefreshExpireMinutes;
				options.ConfirmForward = auth.ConfirmForward;
				options.PublicUrl = auth.PublicUrl;
				options.ResetForward = auth.ResetForward;
			});

			services.Configure<TextServiceSettings>(options => { options.AlphaCode = text.AlpaCode; });

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
			services.AddSqlRepositories();
			services.AddDocumentRepositories(cache.Enabled);

			if(mail.Provider == "SendGrid") {
				services.AddSingleton<IEmailSender, SendGridMailer>();
				services.Configure<SendGridAuthOptions>(opts => {
					opts.FromName = mail.FromName;
					opts.From = mail.From;
					opts.Key = mail.SendGrid.Key;
					opts.Username = mail.SendGrid.Username;
				});
			} else if(mail.Provider == "SMTP") {
				services.AddSingleton<IEmailSender, SmtpMailer>();
				services.Configure<SmtpAuthOptions>(opts => {
					opts.FromName = mail.FromName;
					opts.From = mail.From;
					opts.Password = mail.Smtp.Password;
					opts.Username = mail.Smtp.Username;
					opts.Ssl = mail.Smtp.Ssl;
					opts.Port = mail.Smtp.Port;
					opts.Host = mail.Smtp.Host;
				});
			}

			if(text.Provider == "Twilio") {
				services.AddTwilioTextApi(text.Twilio.AccountSid, text.Twilio.AuthToken);
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
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider sp, ILoggerFactory logger)
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

			logger.AddConsole();
			if(env.IsDevelopment())
				logger.AddDebug();

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
