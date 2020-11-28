/*
 * MQTT service init.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SensateIoT.Platform.Network.Common.MQTT;
using SensateIoT.Platform.Network.Common.Settings;

namespace SensateIoT.Platform.Network.Common.Init
{
	public static class MqttServiceInitExtensions
	{
		public static IServiceCollection AddMqttService(this IServiceCollection service, Action<MqttServiceOptions> setup)
		{
			service.AddSingleton<IHostedService, MqttClient>();

			if(setup != null) {
				service.Configure(setup);
			}

			return service;
		}

		public static IServiceCollection AddMqttHandlers(this IServiceCollection service)
		{
			foreach(var etype in Assembly.GetEntryAssembly().ExportedTypes) {
				if(etype.GetTypeInfo().BaseType == typeof(IMqttHandler)) {
					service.AddScoped(etype);
				}
			}

			return service;
		}

		/*public static IServiceCollection AddCommandPublisher(this IServiceCollection service, Action<CommandPublisherOptions> setup)
		{
			service.Configure(setup);
			service.AddSingleton<IHostedService, CommandPublisher>();

			service.AddSingleton(provider => {
				var s = provider.GetServices<IHostedService>().ToList();
				var mqservice = s.Find(x => x.GetType() == typeof(CommandPublisher)) as ICommandPublisher;
				return mqservice;
			});

			return service;
		}*/

		public static IServiceCollection AddInternalMqttService(this IServiceCollection services, Action<MqttServiceOptions> setup)
		{
			if(setup != null) {
				services.Configure(setup);
			}

			services.AddSingleton<InternalMqttMqttClient>();
			services.AddHostedService(p => p.GetRequiredService<InternalMqttMqttClient>());
			services.AddSingleton<IInternalMqttClient>(p => p.GetRequiredService<InternalMqttMqttClient>());

			return services;
		}

		public static void MapInternalMqttTopic<T>(this IServiceProvider sp, string topic) where T : IMqttHandler
		{
			InternalMqttMqttClient mqttMqtt;
			List<IHostedService> services;

			/*
			 * If anybody knows a cleaner way of going about
			 * IHostedServices: do let me know.
			 *
			 * For now we just get *every* IHostedService and find the one
			 * we need. I'm truly sorry you are a witness to this savage
			 * piece of SWE.
			 */
			services = sp.GetServices<IHostedService>().ToList();
			mqttMqtt = services.Find(x => x.GetType() == typeof(InternalMqttMqttClient)) as InternalMqttMqttClient;
			mqttMqtt?.MapTopicHandler<T>(topic);
		}
	}
}
