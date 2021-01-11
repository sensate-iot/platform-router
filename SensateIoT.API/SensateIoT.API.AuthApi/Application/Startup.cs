/*
 * .NET core services startup.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Microsoft.OpenApi.Models;

using SensateIoT.API.Common.ApiCore.Init;
using SensateIoT.API.Common.ApiCore.Middleware;
using SensateIoT.API.Common.Config.Config;
using SensateIoT.API.Common.Config.Settings;
using SensateIoT.API.Common.Core.Init;
using SensateIoT.API.Common.Core.Services.Adapters;

namespace SensateService.Api.AuthApi.Application
{
	public class Startup
	{
		private readonly IConfiguration _configuration;

		public Startup(IConfiguration configuration)
		{
			this._configuration = configuration;
		}

		// ReSharper disable once UnusedMember.Global
		public void ConfigureServices(IServiceCollection services)
		{
			var cache = new CacheConfig();
			var db = new DatabaseConfig();
			var auth = new AuthenticationConfig();
			var mail = new MailConfig();
			var text = new TextConfig();
			var sys = new SystemConfig();
			var mqtt = new MqttConfig();

			this._configuration.GetSection("Cache").Bind(cache);
			this._configuration.GetSection("Authentication").Bind(auth);
			this._configuration.GetSection("Database").Bind(db);
			this._configuration.GetSection("Mail").Bind(mail);
			this._configuration.GetSection("System").Bind(sys);
			this._configuration.GetSection("Text").Bind(text);
			this._configuration.GetSection("Mqtt").Bind(mqtt);

			services.AddCors();
			var privatemqtt = mqtt.InternalBroker;

			services.AddPostgres(db.PgSQL.ConnectionString, db.Network.ConnectionString);
			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);
			services.AddIdentityFramwork(auth);

			if(cache.Enabled) {
				services.AddCacheStrategy(cache, db);
			}

			services.AddReverseProxy(sys);

			/* Add repositories */
			services.AddSqlRepositories(cache.Enabled);
			services.AddDocumentRepositories(cache.Enabled);
			services.AddSensorServices();
			services.AddUserService();

			services.AddCommandPublisher(options => {
				options.Ssl = privatemqtt.Ssl;
				options.Host = privatemqtt.Host;
				options.Port = privatemqtt.Port;
				options.Username = privatemqtt.Username;
				options.Password = privatemqtt.Password;
				options.CommandsTopic = privatemqtt.InternalCommandsTopic;
			});

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

			if(text.Provider == "Twillio") {
				services.AddTwilioTextApi(text);
			} else {
				Console.WriteLine("Text message provider not configured!");
			}


			services.AddSwaggerGen(c => {
				c.SwaggerDoc("v1", new OpenApiInfo {
					Title = "Sensate IoT Auth API - Version 1",
					Version = "v1"
				});

				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
					In = ParameterLocation.Header,
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey,
					Description = "API key needed to access the endpoints."
				});

				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Name = "Authorization",
							Type = SecuritySchemeType.ApiKey,
							In = ParameterLocation.Header,
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							},
						},
						new string[] {}
					}
				});
			});


			services.AddRouting();
			services.AddControllers().AddNewtonsoftJson();
		}

		// ReSharper disable once UnusedMember.Global
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			var auth = new AuthenticationConfig();
			this._configuration.GetSection("Authentication").Bind(auth);

			app.UseForwardedHeaders();
			app.UseSwagger();

			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseSwagger(c => {
				c.RouteTemplate = "auth/swagger/{documentName}/swagger.json";
			});

			app.UseSwaggerUI(c => {
				c.SwaggerEndpoint("/auth/swagger/v1/swagger.json", "Sensate Auth API v1");
				c.RoutePrefix = "auth/swagger";
			});

			app.UseCors(p => {
				p.SetIsOriginAllowed(host => true)
					.AllowAnyHeader()
					.AllowAnyMethod()
					.AllowCredentials();
			});

			app.UseMiddleware<RequestLoggingMiddleware>();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseMiddleware<UserFetchMiddleware>();
			app.UseEndpoints(ep => { ep.MapControllers(); });
		}
	}
}
