/*
 * MQTT client service factory
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;

using Microsoft.Extensions.DependencyInjection;
using SensateService.Models;
using SensateService.Infrastructure.Repositories;

namespace SensateService.Services
{
	public sealed class MqttServiceFactory
	{
		private MqttServiceFactory()
		{}

		public static MqttService CreateMqttService(IServiceProvider provider, MqttOptions options)
		{
			var mrepo = provider.GetRequiredService<IMeasurementRepository>();
			var srepo = provider.GetRequiredService<ISensorRepository>();
			return new MqttService(srepo, mrepo, options);
		}
	}
}
