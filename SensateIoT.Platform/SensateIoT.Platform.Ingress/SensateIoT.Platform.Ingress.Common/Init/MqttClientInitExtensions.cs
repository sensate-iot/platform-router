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

using SensateIoT.Platform.Ingress.Common.MQTT;
using SensateIoT.Platform.Ingress.Common.Options;

namespace SensateIoT.Platform.Ingress.Common.Init
{
	public static class MqttServiceInitExtensions
	{
		public static IServiceCollection AddMqttService(this IServiceCollection service, Action<MqttServiceOptions> setup)
		{
			service.AddSingleton<MqttClient>();
			service.AddHostedService(p => p.GetRequiredService<MqttClient>());
			service.AddSingleton<IPublicMqttClient>(p => p.GetRequiredService<MqttClient>());

			if(setup != null) {
				service.Configure(setup);
			}

			return service;
		}

		public static IServiceCollection AddMqttHandlers(this IServiceCollection service)
		{
			foreach(var etype in Assembly.GetEntryAssembly().ExportedTypes) {
				if(etype.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IMqttHandler))) {
					service.AddScoped(etype);
				}
			}

			return service;
		}

		public static void MapMqttTopic<T>(this IServiceProvider sp, string topic) where T : IMqttHandler
		{
			MqttClient mqtt;
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
			mqtt = services.Find(x => x.GetType() == typeof(MqttClient)) as MqttClient;
			mqtt?.MapTopicHandler<T>(topic);
		}
	}
}
