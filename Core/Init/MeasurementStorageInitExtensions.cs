/*
 * Measurement storage init extensions.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SensateService.Config;
using SensateService.Infrastructure.Storage;
using SensateService.Services.Processing;

namespace SensateService.Init
{
	public static class MeasurementStorageInitExtensions
	{
		public static IServiceCollection AddMeasurementStorage(this IServiceCollection services, CacheConfig config)
		{
			services.AddSingleton(config);
			services.AddSingleton<IHostedService, MeasurementCacheService>();

			services.AddScoped<IMeasurementStore, MeasurementStore>();
			services.AddSingleton<ICachedMeasurementStore, CachedMeasurementStore>();
			services.AddSingleton<IMeasurementCache>(x => {
				var obj = x.GetRequiredService<ICachedMeasurementStore>();
				return obj;
			});

			return services;
		}
	}
}