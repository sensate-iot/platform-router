/*
 * Repository to deal with cache misses.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System.Threading.Tasks;

using SensateService.Infrastructure.Cache;
using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class CachedApiKeyRepository : ApiKeyRepository
	{
		private readonly ICacheStrategy<string> m_cache;

		public CachedApiKeyRepository(SensateSqlContext context, ICacheStrategy<string> cache) : base(context)
		{
			this.m_cache = cache;
		}

		public override Task CreateAsync(SensateApiKey key, CancellationToken token = default)
		{
			var asyncio = new Task[2];

			asyncio[0] = base.CreateAsync(key, token);
			asyncio[1] = this.m_cache.RemoveAsync(key.UserId);

			return Task.WhenAll(asyncio);
		}

		public override Task CreateSensorKey(SensateApiKey key, Sensor sensor, CancellationToken token = default)
		{
			var asyncio = new Task[2];

			asyncio[0] = base.CreateSensorKey(key, sensor, token);
			asyncio[1] = this.m_cache.RemoveAsync(key.UserId);

			return Task.WhenAll(asyncio);
		}
	}
}
