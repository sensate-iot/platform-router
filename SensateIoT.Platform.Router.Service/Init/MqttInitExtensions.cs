using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SensateIoT.Platform.Router.Common.Init;
using SensateIoT.Platform.Router.Service.Config;

namespace SensateIoT.Platform.Router.Service.Init
{
	public static class MqttInitExtensions
	{
		public static void AddMqttBrokers(this IServiceCollection services, IConfiguration configuration)
		{
			var id = configuration.GetValue<string>("ApplicationId");
			var mqtt = new MqttConfig();

			configuration.GetSection("Mqtt").Bind(mqtt);
			var privatemqtt = mqtt.InternalBroker;
			var publicmqtt = mqtt.PublicBroker;

			services.AddInternalMqttService(options => {
				options.Ssl = privatemqtt.Ssl;
				options.Host = privatemqtt.Host;
				options.Port = privatemqtt.Port;
				options.Username = privatemqtt.Username;
				options.Password = privatemqtt.Password;
				options.Id = $"router-{id}";
			});

			services.AddMqttService(options => {
				options.Ssl = publicmqtt.Ssl;
				options.Host = publicmqtt.Host;
				options.Port = publicmqtt.Port;
				options.Username = publicmqtt.Username;
				options.Password = publicmqtt.Password;
				options.Id = $"router-{id}-{Guid.NewGuid():N}";
			});

		}
	}
}
