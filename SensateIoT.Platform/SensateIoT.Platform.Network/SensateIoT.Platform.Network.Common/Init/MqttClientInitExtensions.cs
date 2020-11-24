/*
 * MQTT service init.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SensateIoT.Platform.Network.Common.Infrastructure;
using SensateIoT.Platform.Network.Common.Settings;

namespace SensateService.Init
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
				if(etype.GetTypeInfo().BaseType == typeof(MqttHandler)) {
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

			services.AddSingleton<InternalMqttService>();
			services.AddHostedService(p => p.GetRequiredService<InternalMqttService>());
			//services.AddSingleton<IMqttPublishService>(p => p.GetRequiredService<InternalMqttClient>());

			return services;
		}

		public static void MapInternalMqttTopic<T>(this IServiceProvider sp, string topic) where T : MqttHandler
		{
			InternalMqttService mqtt;
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
			mqtt = services.Find(x => x.GetType() == typeof(InternalMqttService)) as InternalMqttService;
			mqtt?.MapTopicHandler<T>(topic);
		}
	}
}
