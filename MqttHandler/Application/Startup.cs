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

using SensateService.Config;
using SensateService.Init;
using SensateService.MqttHandler.Mqtt;

namespace SensateService.MqttHandler.Application
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
			var sys = new SystemConfig();

			this.Configuration.GetSection("Mqtt").Bind(mqtt);
			this.Configuration.GetSection("System").Bind(sys);
			var publicmqtt = mqtt.PublicBroker;

			services.AddLogging(builder => { builder.AddConfiguration(this.Configuration.GetSection("Logging")); });
			services.AddAuthorizationProxy(sys);
			services.AddMqttService(options => {
				options.Ssl = publicmqtt.Ssl;
				options.Host = publicmqtt.Host;
				options.Port = publicmqtt.Port;
				options.Username = publicmqtt.Username;
				options.Password = publicmqtt.Password;
				options.Id = Guid.NewGuid().ToString();
				options.TopicShare = "$share/sensate/";
			});

			services.AddLogging(builder => {
				builder.AddConfiguration(this.Configuration.GetSection("Logging"));

				if(IsDevelopment()) {
					builder.AddDebug();
				}

				builder.AddConsole();
			});
		}

		public void Configure(IServiceProvider provider)
		{
			var mqtt = new MqttConfig();

			this.Configuration.GetSection("Mqtt").Bind(mqtt);
			var @public = mqtt.PublicBroker;

			provider.MapMqttTopic<MqttMeasurementHandler>(@public.MeasurementTopic);
			provider.MapMqttTopic<MqttBulkMeasurementHandler>(@public.BulkMeasurementTopic);
			provider.MapMqttTopic<MqttBulkMessageHandler>(@public.BulkMessageTopic);
			provider.MapMqttTopic<MqttMessageHandler>(@public.MessageTopic);
		}
	}
}

