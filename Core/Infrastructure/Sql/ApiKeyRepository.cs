/*
 * API key data layer implementation.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class ApiKeyRepository : AbstractSqlRepository<SensateApiKey>, IApiKeyRepository
	{
		private Random _rng;
		private const int UserTokenLength = 32;

		public ApiKeyRepository(SensateSqlContext context) : base(context)
		{
			this._rng = new Random(DateTime.Now.Millisecond);
		}

		public Task CreateAsync(SensateApiKey key, CancellationToken token = default(CancellationToken))
		{
			key.ApiKey = this._rng.NextStringWithSymbols(UserTokenLength);
			this.Data.Add(key);
			return this._sqlContext.SaveChangesAsync(token);
		}

		public async Task<SensateApiKey> GetByKeyAsync(string key, CancellationToken token = default(CancellationToken))
		{
			var query = this.Data.Where(apikey => apikey.ApiKey == key).Include(apikey => apikey.User)
				.ThenInclude(user => user.ApiKeys);
			var _apikey = await query.FirstOrDefaultAsync(token).AwaitBackground();

			if(_apikey == null)
				return null;

			_apikey.CreatedOn = DateTime.SpecifyKind(_apikey.CreatedOn, DateTimeKind.Utc);

			return _apikey;
		}

		public async Task<SensateApiKey> GetByIdAsync(string id, CancellationToken token = default(CancellationToken))
		{
			var query = this.Data.Where(apikey => apikey.Id == id).Include(apikey => apikey.User)
				.ThenInclude(user => user.ApiKeys);
			var _apikey = await query.FirstOrDefaultAsync(token).AwaitBackground();

			if(_apikey == null)
				return null;

			_apikey.CreatedOn = DateTime.SpecifyKind(_apikey.CreatedOn, DateTimeKind.Utc);

			return _apikey;
		}

		public Task MarkRevokedAsync(SensateApiKey key, CancellationToken token = default(CancellationToken))
		{
			this.StartUpdate(key);
			key.Revoked = true;
			return this.EndUpdateAsync();
		}

		public Task MarkRevokedRangeAsync(IEnumerable<SensateApiKey> keys, CancellationToken token = default(CancellationToken))
		{
			var apikeys = keys.ToList();

			if(apikeys.Count <= 0)
				return Task.CompletedTask;

			this._sqlContext.UpdateRange(apikeys);

			foreach(var key in apikeys) {
				key.Revoked = true;
			}

			return this._sqlContext.SaveChangesAsync(token);
		}

		public async Task<SensateApiKey> RefreshAsync(SensateApiKey apikey, CancellationToken token = default(CancellationToken))
		{
			this.StartUpdate(apikey);
			apikey.ApiKey = this._rng.NextStringWithSymbols(UserTokenLength);
			await this.EndUpdateAsync().AwaitBackground();

			return apikey;
		}

		public async Task<SensateApiKey> RefreshAsync(string id, CancellationToken token = default(CancellationToken))
		{
			var apikey = await this.GetByIdAsync(id, token).AwaitBackground();

			if(apikey == null || apikey.Revoked)
				return null;

			return await this.RefreshAsync(apikey, token).AwaitBackground();
		}

		public async Task<IEnumerable<SensateApiKey>> GetByUserAsync(SensateUser user, CancellationToken token = default(CancellationToken))
		{
			var query = this.Data.Where(apikey => apikey.UserId == user.Id).Include(apikey => apikey.User)
				.ThenInclude(u => u.ApiKeys);
			var keys = await query.ToListAsync(token).AwaitBackground();

			if(keys == null)
				return null;

			foreach(var key in keys) {
				key.CreatedOn = DateTime.SpecifyKind(key.CreatedOn, DateTimeKind.Utc);
			}

			return keys;
		}

		public async Task<IEnumerable<SensateApiKey>> GetByUserAsync(SensateUser user, ApiKeyType type, CancellationToken token = default(CancellationToken))
		{
			var query = this.Data.Where(apikey => apikey.UserId == user.Id && apikey.Type == type).Include(apikey => apikey.User)
				.ThenInclude(u => u.ApiKeys);
			var keys = await query.ToListAsync(token).AwaitBackground();

			if(keys == null)
				return null;

			foreach(var key in keys) {
				key.CreatedOn = DateTime.SpecifyKind(key.CreatedOn, DateTimeKind.Utc);
			}

			return keys;
		}
	}
}
