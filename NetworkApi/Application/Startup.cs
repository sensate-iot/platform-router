/*
 * .NET core services startup.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using SensateService.ApiCore.Init;
using SensateService.ApiCore.Middleware;
using SensateService.Config;
using SensateService.Infrastructure.Sql;
using SensateService.Init;
using SensateService.NetworkApi.Mqtt;
using SensateService.Services;
using SensateService.Services.Adapters;
using SensateService.Services.Settings;

namespace SensateService.NetworkApi.Application
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
			var mail = new MailConfig();
			var text = new TextConfig();
			var auth = new AuthenticationConfig();
			var mqtt = new MqttConfig();

			this._configuration.GetSection("Mqtt").Bind(mqtt);
			this._configuration.GetSection("Authentication").Bind(auth);
			this._configuration.GetSection("Cache").Bind(cache);
			this._configuration.GetSection("Database").Bind(db);
			this._configuration.GetSection("Mail").Bind(mail);
			this._configuration.GetSection("Text").Bind(text);

			var publicmqtt = mqtt.PublicBroker;

			services.AddCors();

			services.AddPostgres(db.PgSQL.ConnectionString);
			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);
			services.AddIdentityFramwork(auth);

			if(cache.Enabled) {
				services.AddCacheStrategy(cache, db);
			}

			/* Add repositories */
			services.AddSqlRepositories(cache.Enabled);
			services.AddDocumentRepositories(cache.Enabled);
			services.AddMeasurementStorage(cache);
			services.AddSensorServices();
			services.AddHashAlgorihms();

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

			services.AddMqttPublishService<MqttPublishService>(options => {
				options.Ssl = publicmqtt.Ssl;
				options.Host = publicmqtt.Host;
				options.Port = publicmqtt.Port;
				options.Username = publicmqtt.Username;
				options.Password = publicmqtt.Password;
				options.Id = Guid.NewGuid().ToString();
				options.ActuatorTopic = publicmqtt.ActuatorTopic;
			});

			if(text.Provider == "Twillio") {
				services.AddTwilioTextApi(text);
			} else {
				Console.WriteLine("Text message provider not configured!");
			}

			services.AddSwaggerGen(c => {
				c.SwaggerDoc("v1", new OpenApiInfo {
					Title = "Sensate Data API - Version 1",
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
			var cache = new CacheConfig();

			this._configuration.GetSection("Authentication").Bind(auth);
			this._configuration.GetSection("Cache").Bind(cache);

			app.UseRouting();

			app.UseCors(p => {
				p.SetIsOriginAllowed(host => true)
					.AllowAnyHeader()
					.AllowAnyMethod()
					.AllowCredentials();
			});

			using(var scope = sp.CreateScope()) {
				var ctx = scope.ServiceProvider.GetRequiredService<SensateSqlContext>();
				ctx.Database.EnsureCreated();
				ctx.Database.Migrate();
			}

			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseSwagger(c =>
			{
				c.RouteTemplate = "network/swagger/{documentName}/swagger.json";
			});

			app.UseSwaggerUI(c => {
				c.SwaggerEndpoint("/network/swagger/v1/swagger.json", "Sensate Network API v1");
				c.RoutePrefix = "network/swagger";
			});

			app.UseMiddleware<ApiKeyValidationMiddleware>();
			app.UseMiddleware<RequestLoggingMiddleware>();

			app.UseAuthentication();
			app.UseAuthorization();
			app.UseEndpoints(ep => { ep.MapControllers(); });
		}
	}
}

