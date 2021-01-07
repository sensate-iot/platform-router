/*
 * Program startup.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SensateIoT.Platform.Network.Common.Init;
using SensateIoT.Platform.Network.Common.Services.Metrics;
using SensateIoT.Platform.Network.Common.Settings;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.DataAccess.Repositories;
using SensateIoT.Platform.Network.StorageService.Config;
using SensateIoT.Platform.Network.StorageService.MQTT;
using SensateIoT.Platform.Network.TriggerService.Config;

namespace SensateIoT.Platform.Network.StorageService.Application
{
	public class Startup
	{
		private readonly IConfiguration Configuration;

		public Startup(IConfiguration configuration)
		{
			this.Configuration = configuration;
		}

		private static bool IsDevelopment()
		{
#if DEBUG
			var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
			return env == "Development";
#else
			return false;
#endif

		}

		public void ConfigureServices(IServiceCollection services)
		{
			var mqtt = new MqttConfig();
			var db = new DatabaseConfig();

			this.Configuration.GetSection("Mqtt").Bind(mqtt);
			this.Configuration.GetSection("Database").Bind(db);

			var privatemqtt = mqtt.InternalBroker;

			services.AddDocumentStore(db.MongoDB.ConnectionString, db.MongoDB.DatabaseName, db.MongoDB.MaxConnections);
			services.AddScoped<IMeasurementRepository, MeasurementRepository>();
			services.AddScoped<ISensorStatisticsRepository, SensorStatisticsRepository>();
			services.AddScoped<IMessageRepository, MessageRepository>();

			services.Configure<MetricsOptions>(this.Configuration.GetSection("HttpServer:Metrics"));
			services.AddHostedService<MetricsService>();

			services.AddInternalMqttService(options => {
				options.Ssl = privatemqtt.Ssl;
				options.Host = privatemqtt.Host;
				options.Port = privatemqtt.Port;
				options.Username = privatemqtt.Username;
				options.Password = privatemqtt.Password;
				options.Id = Guid.NewGuid().ToString();
				options.TopicShare = "$share/sensateiot-storage/";
			});

			services.AddLogging(builder => {
				builder.AddConfiguration(this.Configuration.GetSection("Logging"));

				if(IsDevelopment()) {
					builder.AddDebug();
				}

				builder.AddConsole();
			});

			services.AddMqttHandlers();
		}

		public void Configure(IServiceProvider provider)
		{
			var mqtt = new MqttConfig();

			this.Configuration.GetSection("Mqtt").Bind(mqtt);
			var @private = mqtt.InternalBroker;

			provider.MapInternalMqttTopic<MqttBulkMeasurementHandler>(@private.BulkMeasurementTopic);
			provider.MapInternalMqttTopic<MqttBulkMessageHandler>(@private.BulkMessageTopic);
		}
	}
}
