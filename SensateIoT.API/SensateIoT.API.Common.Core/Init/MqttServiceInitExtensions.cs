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
using SensateIoT.API.Common.Config.Settings;
using SensateIoT.API.Common.Core.Middleware;
using SensateIoT.API.Common.Core.Services.Processing;

namespace SensateIoT.API.Common.Core.Init
{
	public static class MqttServiceInitExtensions
	{
		public static IServiceCollection AddMqttService(this IServiceCollection service, Action<MqttServiceOptions> setup)
		{
			service.AddSingleton<IHostedService, MqttService>();

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

		public static IServiceCollection AddCommandPublisher(this IServiceCollection service, Action<CommandPublisherOptions> setup)
		{
			service.Configure(setup);
			service.AddSingleton<IHostedService, CommandPublisher>();

			service.AddSingleton(provider => {
				var s = provider.GetServices<IHostedService>().ToList();
				var mqservice = s.Find(x => x.GetType() == typeof(CommandPublisher)) as ICommandPublisher;
				return mqservice;
			});

			return service;
		}

		public static IServiceCollection AddMqttPublishService<T>(this IServiceCollection services, Action<MqttPublishServiceOptions> setup = null)
			where T : AbstractMqttService, IMqttPublishService
		{
			services.AddSingleton<IHostedService, T>();

			if(setup != null)
				services.Configure(setup);

			services.AddSingleton(provider => {
				var s = provider.GetServices<IHostedService>().ToList();
				var mqservice = s.Find(x => x.GetType() == typeof(T)) as IMqttPublishService;
				return mqservice;
			});

			return services;
		}

		public static IServiceCollection AddInternalMqttService(this IServiceCollection services, Action<InternalMqttServiceOptions> setup)
		{
			if(setup != null) {
				services.Configure(setup);
			}

			services.AddSingleton<InternalMqttService>();
			services.AddHostedService(p => p.GetRequiredService<InternalMqttService>());
			services.AddSingleton<IMqttPublishService>(p => p.GetRequiredService<InternalMqttService>());

			return services;
		}

		public static void MapMqttTopic<T>(this IServiceProvider sp, string topic) where T : MqttHandler
		{
			MqttService mqtt;
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
			mqtt = services.Find(x => x.GetType() == typeof(MqttService)) as MqttService;
			mqtt?.MapTopicHandler<T>(topic);
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
