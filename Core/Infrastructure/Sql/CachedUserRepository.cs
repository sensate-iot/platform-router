/*
 * Memory cache repository for the UserRepository class.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

using Newtonsoft.Json;

using SensateService.Helpers;
using SensateService.Infrastructure.Cache;
using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class CachedUserRepository : UserRepository
	{
		private readonly ICacheStrategy<string> _cache;

		public CachedUserRepository(SensateSqlContext context, UserManager<SensateUser> manager,
			ICacheStrategy<string> cache) : base(context, manager)
		{
			this._cache = cache;
		}

		private Task SetCacheAsync(SensateUser user, CancellationToken ct)
		{
			return this._cache.SetAsync(
				user.Id, JsonConvert.SerializeObject(user),
				CacheTimeout.TimeoutShort.ToInt(),
				false, ct);
		}

		private async Task<SensateUser> LookupAsync(string id, CancellationToken ct)
		{
			var obj = await this._cache.GetAsync(id, ct).AwaitSafely();

			if(obj == null)
				return null;

			return JsonConvert.DeserializeObject<SensateUser>(obj);
		}
		private void SetCache(SensateUser user)
		{
			this._cache.Set(
				user.Id, JsonConvert.SerializeObject(user),
				CacheTimeout.TimeoutShort.ToInt(),
				false);
		}

		private SensateUser Lookup(string id)
		{
			var obj = this._cache.Get(id);

			if(obj == null)
				return null;

			return JsonConvert.DeserializeObject<SensateUser>(obj);
		}


		public override SensateUser Get(string key)
		{
			var obj = this.Lookup(key);

			if(obj != null)
				return obj;

			obj = base.Get(key);
			if(obj == null)
				return null;

			this.SetCache(obj);
			return obj;
		}

		public override async Task<SensateUser> GetAsync(string key)
		{
			var obj = await this.LookupAsync(key, CancellationToken.None).AwaitSafely();

			if(obj != null) {
				return obj;
			}

			obj = await base.GetAsync(key).AwaitSafely();
			if(obj == null)
				return null;

			await this.SetCacheAsync(obj, CancellationToken.None);
			return obj;
		}

		public override void Delete(string id)
		{
			this._cache.Remove(id);
			base.Delete(id);
		}

		public override async Task DeleteAsync(string id)
		{
			Task[] workers = new Task[2];

			workers[0] = this._cache.RemoveAsync(id);
			workers[1] = base.DeleteAsync(id);
			await Task.WhenAll(workers);
		}
	}
}
