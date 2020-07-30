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
using Microsoft.Extensions.Logging;

using SensateService.ApiCore.Init;
using SensateService.Config;
using SensateService.Init;
using SensateService.WebSocketHandler.Handlers;

namespace SensateService.WebSocketHandler.Application
{
	public class ApplicationStarter
	{
		private readonly IConfiguration Configuration;

		public ApplicationStarter(IConfiguration config)
		{
			this.Configuration = config;
		}

		private static bool IsDevelopment()
		{
#if DEBUG
			var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
			return env == "Development";
#else
			return false;
#endif

		}

		public void ConfigureServices(IServiceCollection services)
		{
			var mqtt = new MqttConfig();
			var db = new DatabaseConfig();
			var cache = new CacheConfig();
			var auth = new AuthenticationConfig();
			var sys = new SystemConfig();

			this.Configuration.GetSection("System").Bind(sys);
			this.Configuration.GetSection("Mqtt").Bind(mqtt);
			this.Configuration.GetSection("Database").Bind(db);
			this.Configuration.GetSection("Authentication").Bind(auth);
			this.Configuration.GetSection("Cache").Bind(cache);

			services.AddPostgres(db.PgSQL.ConnectionString);
			services.AddLogging(builder => { builder.AddConfiguration(this.Configuration.GetSection("Logging")); });

			if(cache.Enabled) {
				services.AddCacheStrategy(cache, db);
			}

			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);
			services.AddDocumentRepositories(cache.Enabled);
			services.AddSqlRepositories(cache.Enabled);
			services.AddMeasurementStorage(cache);
			services.AddIdentityFramwork(auth);
			services.AddHashAlgorihms();
			services.AddReverseProxy(sys);

			services.AddInternalMqttService(options => {
				options.Ssl = mqtt.InternalBroker.Ssl;
				options.Host = mqtt.InternalBroker.Host;
				options.Port = mqtt.InternalBroker.Port;
				options.Username = mqtt.InternalBroker.Username;
				options.Password = mqtt.InternalBroker.Password;
				options.Id = Guid.NewGuid().ToString();
				options.InternalBulkMeasurementTopic = mqtt.InternalBroker.InternalBulkMeasurementTopic;
				options.InternalBulkMessageTopic = mqtt.InternalBroker.InternalBulkMessageTopic;
			});

			services.AddSingleton<IHostedService, MqttPublishHandler>();

			services.AddWebSocketHandler<WebSocketMessageHandler>();
			services.AddWebSocketHandler<WebSocketMeasurementHandler>();
			services.AddControllers().AddNewtonsoftJson();

			services.AddLogging((builder) => {
				builder.AddConsole();

				if(IsDevelopment()) {
					builder.AddDebug();
				}
			});
		}

		public void Configure(IApplicationBuilder app, IServiceProvider sp)
		{
			var cache = new CacheConfig();

			app.UseForwardedHeaders();
			app.UseRouting();
			app.UseWebSockets();

			this.Configuration.GetSection("Cache").Bind(cache);

			app.MapWebSocketService("/ingress/v1/message", sp.GetService<WebSocketMessageHandler>());
			app.MapWebSocketService("/ingress/v1/measurement", sp.GetService<WebSocketMeasurementHandler>());

			app.UseAuthentication();
			app.UseAuthorization();
			app.UseEndpoints(ep => { ep.MapControllers(); });
		}
	}
}
