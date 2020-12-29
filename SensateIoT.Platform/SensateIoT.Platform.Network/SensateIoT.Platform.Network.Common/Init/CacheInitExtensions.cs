/*
 * Caching init extensions.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Redis;

namespace SensateIoT.Platform.Network.Common.Init
{
	public static class CacheInitExtensions
	{
		public static IServiceCollection AddDistributedCaches<TValue>(this IServiceCollection services, string host, int port) where TValue : class
		{
			services.AddSingleton<IDistributedCache<TValue>>(p => {
				var opts = new DistributedCacheOptions {
					Configuration = new ConfigurationOptions {
						EndPoints = { { host, port } },
						ClientName = "SensateIoT"
					}
				};

				return new RedisCache<TValue>(opts);
			});
			return services;
		}
	}
}
