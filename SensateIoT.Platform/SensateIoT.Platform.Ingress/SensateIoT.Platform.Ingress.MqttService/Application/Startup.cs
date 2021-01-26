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

using SensateIoT.Platform.Ingress.Common.Init;
using SensateIoT.Platform.Ingress.Common.Options;
using SensateIoT.Platform.Ingress.DataAccess.Init;
using SensateIoT.Platform.Ingress.MqttService.Mqtt;

namespace SensateIoT.Platform.Ingress.MqttService.Application
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
			var connString = this.Configuration.GetValue<string>("Database:MongoDB:ConnectionString");
			var connectionCount = this.Configuration.GetValue<int>("Database:MongoDB:MaxConnections");
			var dbName = this.Configuration.GetValue<string>("Database:MongoDB:DatabaseName");

			this.Configuration.GetSection("Mqtt").Bind(mqtt);

			services.AddMongoDb(connString, dbName, connectionCount);
			services.AddLogging(builder => { builder.AddConfiguration(this.Configuration.GetSection("Logging")); });
			services.Configure<GatewaySettings>(this.Configuration.GetSection("Gateway"));

			services.AddMqttService(options => {
				options.Ssl = mqtt.Ssl;
				options.Host = mqtt.Host;
				options.Port = mqtt.Port;
				options.Username = mqtt.Username;
				options.Password = mqtt.Password;
				options.Id = Guid.NewGuid().ToString();
				options.TopicShare = "$share/ingress/";
			});

			services.AddLogging(builder => {
				builder.AddConfiguration(this.Configuration.GetSection("Logging"));

				if(IsDevelopment()) {
					builder.AddDebug();
				}

				builder.AddConsole();
			});

			services.AddMqttHandlers();
			services.AddHttpClient();
		}

		public void Configure(IServiceProvider provider)
		{
			var @public = new MqttConfig();

			this.Configuration.GetSection("Mqtt").Bind(@public);

			provider.MapMqttTopic<MqttMeasurementHandler>(@public.MeasurementTopic);
			provider.MapMqttTopic<MqttMessageHandler>(@public.MessageTopic);
		}
	}
}

