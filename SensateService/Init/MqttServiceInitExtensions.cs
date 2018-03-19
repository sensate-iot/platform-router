/*
 * MQTT service init.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SensateService.Middleware;
using SensateService.Services;

namespace SensateService.Init
{
	public static class MqttServiceInitExtensions
	{
		public static IServiceCollection AddMqttService(
			this IServiceCollection service,
			Action<MqttServiceOptions> setup
		)
		{
			service.AddSingleton<IHostedService, MqttService>();

			if(setup != null)
				service.Configure<MqttServiceOptions>(setup);

			foreach(var etype in Assembly.GetEntryAssembly().ExportedTypes) {
				if(etype.GetTypeInfo().BaseType == typeof(MqttHandler))
					service.AddScoped(etype);
			}

			return service;
		}

		public static void MapMqttTopic<T>(
			this IApplicationBuilder app,
			IServiceProvider sp,
			string topic
		) where T : MqttHandler
		{
			MqttService mqtt;
			var services = sp.GetServices<IHostedService>();

			/*
			 * If anybody knows a cleaner way of going about
			 * IHostedServices: do let me know.
			 *
			 * For now we just get *every* IHostedService and find the one
			 * we need. I'm truly sorry you are a witness to this savage
			 * piece of SWE.
			 */
			foreach(var service in services) {
				if(service is MqttService) {
					mqtt = service as MqttService;
					mqtt.MapTopicHandler<T>(topic);
				}
			}
		}
	}
}
