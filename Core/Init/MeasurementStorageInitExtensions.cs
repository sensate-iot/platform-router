/*
 * Measurement storage init extensions.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SensateService.Infrastructure.Storage;
using SensateService.Services;
using SensateService.Services.Processing;

namespace SensateService.Init
{
	public static class MeasurementStorageInitExtensions
	{
		public static IServiceCollection AddMeasurementStorage(this IServiceCollection services)
		{
			services.AddSingleton<IHostedService, MeasurementCacheService>();
			services.AddSingleton(provider => {
				var s = provider.GetServices<IHostedService>().ToList();
				return s.Find(x => x.GetType() == typeof(MeasurementCacheService)) as IMeasurementCacheService;
			});

			services.AddScoped<IMeasurementStore, MeasurementStore>();
			services.AddScoped(provider => {
				var service = provider.GetRequiredService<IMeasurementCacheService>();
				return service.Next();
			});

			return services;
		}

		public static IServiceProvider UseMeasurementStorage(this IServiceProvider provider, int workers)
		{
			var service = provider.GetRequiredService<IMeasurementCacheService>();

			for(var num = 0; num < workers; num++) {
				var logger = provider.GetRequiredService<ILogger<CachedMeasurementStore>>();
				var cache = new CachedMeasurementStore(provider.CreateScope().ServiceProvider, logger);

				service.RegisterCache(cache);
			}

			return provider;
		}
	}
}