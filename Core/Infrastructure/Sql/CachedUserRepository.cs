/*
 * Memory cache repository for the UserRepository class.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Collections.Generic;
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
			var obj = await this._cache.GetAsync(id, ct).AwaitBackground();

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
			var obj = await this.LookupAsync(key, default(CancellationToken)).ConfigureAwait(false);

			if(obj != null) {
				return obj;
			}

			obj = await base.GetAsync(key).AwaitBackground();
			if(obj == null)
				return null;

			await this.SetCacheAsync(obj, default(CancellationToken)).ConfigureAwait(false);
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

		public override async Task<IEnumerable<string>> GetRolesAsync(SensateUser user)
		{
			var id = $"{{roles}}:{user.Id}";
			var obj = await this._cache.GetAsync(id, CancellationToken.None).ConfigureAwait(false);

			if(obj != null)
				return JsonConvert.DeserializeObject<IEnumerable<string>>(obj);

			var roles = await base.GetRolesAsync(user).ConfigureAwait(false);

			if(roles != null) {
				obj = JsonConvert.SerializeObject(roles);
				await this._cache.SetAsync(id, obj, CacheTimeout.TimeoutShort.ToInt(), false).ConfigureAwait(false);
			}

			return roles;
		}
	}
}
