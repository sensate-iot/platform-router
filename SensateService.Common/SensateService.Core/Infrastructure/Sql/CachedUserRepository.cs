/*
 * Memory cache repository for the UserRepository class.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

using Newtonsoft.Json;
using SensateService.Enums;
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

			if(obj == null) {
				return null;
			}

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

			if(obj == null) {
				return null;
			}

			return JsonConvert.DeserializeObject<SensateUser>(obj);
		}


		public override SensateUser Get(string key)
		{
			var obj = this.Lookup(key);

			if(obj != null)
				return obj;

			obj = base.Get(key);

			if(obj == null) {
				return null;
			}

			this.SetCache(obj);
			return obj;
		}

		public override async Task<SensateUser> GetAsync(string key, bool withKeys)
		{
			var obj = await this.LookupAsync($"{key}:{withKeys}", default).ConfigureAwait(false);

			if(obj != null) {
				var entries = this._sqlContext.ChangeTracker.Entries<SensateUser>().ToList();

				foreach(var entry in entries) {
					if(entry.Entity.Id != key) {
						continue;
					}

					return obj;
				}

				return obj;
			}

			obj = await base.GetAsync(key, withKeys).AwaitBackground();

			if(obj == null) {
				return null;
			}

			await this.SetCacheAsync(obj, default).ConfigureAwait(false);
			await this._cache.SetAsync(
				$"{obj.Id}:{withKeys}", JsonConvert.SerializeObject(obj),
				CacheTimeout.TimeoutShort.ToInt(),
				false
				);
			return obj;
		}

		public override async Task<IEnumerable<SensateUser>> GetAllAsync(int skip = 0, int limit = 0, CancellationToken ct = default)
		{
			var key = $"all_users::{skip}::{limit}";
			var obj = await this._cache.GetAsync(key, ct).AwaitBackground();

			if(obj != null) {
				return JsonConvert.DeserializeObject<SensateUser[]>(obj);
			}

			var users = await base.GetAllAsync(skip, limit, ct).AwaitBackground();
			obj = JsonConvert.SerializeObject(users);
			await this._cache.SetAsync(key, obj, CacheTimeout.Timeout.ToInt(), false, ct);

			return users;
		}

		public override async Task DeleteAsync(string id, CancellationToken ct = default)
		{
			var workers = new Task[3];

			workers[0] = this._cache.RemoveAsync(id);
			workers[1] = this._cache.RemoveAsync($"all_users::0::0");
			workers[2] = base.DeleteAsync(id, ct);
			await Task.WhenAll(workers);
		}

		public override async Task<IEnumerable<string>> GetRolesAsync(SensateUser user)
		{
			var id = $"user_roles:{user.Id}";
			var obj = await this._cache.GetAsync(id, CancellationToken.None).ConfigureAwait(false);

			if(obj != null) {
				return JsonConvert.DeserializeObject<IEnumerable<string>>(obj);
			}

			var roles = await base.GetRolesAsync(user).ConfigureAwait(false);

			if(roles != null) {
				obj = JsonConvert.SerializeObject(roles);
				await this._cache.SetAsync(id, obj, CacheTimeout.TimeoutShort.ToInt(), false).ConfigureAwait(false);
			}

			return roles;
		}
	}
}
