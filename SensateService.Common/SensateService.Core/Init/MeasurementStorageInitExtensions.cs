/*
 * Measurement storage init extensions.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
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
			services.AddSingleton<IHostedService, CacheService>();

			services.AddSingleton<ICachedMeasurementStore, CachedMeasurementStore>();
			services.AddSingleton<IMeasurementCache>(x => {
				var obj = x.GetRequiredService<ICachedMeasurementStore>();
				return obj;
			});

			return services;
		}

		public static IServiceCollection AddMessageStorage(this IServiceCollection services)
		{
			services.AddSingleton<ICachedMessageStore, CachedCachedMessageStore>();
			services.AddSingleton<IMessageCache>(x => {
				var obj = x.GetRequiredService<ICachedMessageStore>();
				return obj;
			});

			return services;
		}
	}
}