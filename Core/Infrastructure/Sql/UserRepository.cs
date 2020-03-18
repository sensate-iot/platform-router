/*
 * User repository interface.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SensateService.Constants;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class UserRepository : AbstractSqlRepository<SensateUser>, IUserRepository
	{
		private readonly UserManager<SensateUser> _manager;

		public UserRepository(SensateSqlContext context, UserManager<SensateUser> manager) : base(context)
		{
			this._manager = manager;
		}

		public override void Create(SensateUser obj) => throw new SystemException("UserRepository.Create is forbidden!");

		public virtual async Task DeleteAsync(string id, CancellationToken ct = default)
		{
			var user = await this._manager.FindByIdAsync(id).AwaitBackground();

			if(ct.IsCancellationRequested)
				throw new OperationCanceledException();

			await this._manager.DeleteAsync(user).AwaitBackground();
		}

		public virtual SensateUser GetById(string id) => string.IsNullOrEmpty(id) ? null : this.Data.FirstOrDefault(x => x.Id == id);
		public SensateUser GetByEmail(string email) => this.Data.FirstOrDefault(x => x.Email == email);

		public async Task<SensateUser> GetByEmailAsync(string email, CancellationToken ct = default)
		{
			return await this._manager.FindByEmailAsync(email).AwaitBackground();
		}

		public Task<IEnumerable<SensateUser>> GetRangeAsync(IEnumerable<string> ids)
		{
			var worker = Task.Run(() => {
				var profiles = this.Data.Where(u => ids.Contains(u.Id))
					.Include(u => u.ApiKeys).ThenInclude(key => key.User)
					.Include(u => u.UserRoles).ThenInclude(ur => ur.Role);
				return profiles.ToList().AsEnumerable();
			});

			return worker;
		}

		public virtual async Task<IEnumerable<SensateUser>> GetAllAsync(int skip = 0, int limit = 0, CancellationToken ct = default)
		{
			var users = this.Data.AsQueryable();

			if(skip > 0) {
				users = users.Skip(skip);
			}

			if(limit > 0) {
				users = users.Take(limit);
			}

			return await users.ToListAsync(ct).AwaitBackground();
		}

		public virtual SensateUser Get(string key)
		{
			return this.GetById(key);
		}

		public virtual Task<SensateUser> GetAsync(string key)
		{
			var worker = Task.Run(() => {
				return this.Data.Where(u => u.Id == key)
					.Include(u => u.ApiKeys).ThenInclude(k => k.User)
					.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefault();
			});

			return worker;
		}

		public SensateUser GetByClaimsPrinciple(ClaimsPrincipal cp)
		{
			string id;

			id = this._manager.GetUserId(cp);
			return this.GetById(id);
		}

		public async Task<SensateUser> GetByClaimsPrincipleAsync(ClaimsPrincipal cp)
		{
			string id;

			id = this._manager.GetUserId(cp);

			if(id == null)
				return null;

			return await this._manager.FindByIdAsync(id).AwaitBackground();
		}

		public IEnumerable<string> GetRoles(SensateUser user)
		{
			var result = this._manager.GetRolesAsync(user);
			return result.Result;
		}

		public virtual async Task<IEnumerable<string>> GetRolesAsync(SensateUser user)
		{
			return await this._manager.GetRolesAsync(user).AwaitBackground();
		}

		public async Task<IEnumerable<SensateUser>> FindByEmailAsync(string email, int skip = 0, int limit = 0, CancellationToken ct = default)
		{
			var result = this.Data.Where(x => x.Email.Contains(email));

			if(skip > 0) {
				result = result.Skip(skip);
			}

			if(limit > 0) {
				result = result.Take(limit);
			}

			return await result.ToListAsync(ct).AwaitBackground();
		}

		public async Task<int> CountAsync()
		{
			return await this.Data.CountAsync().AwaitBackground();
		}

		public async Task<int> CountGhostUsersAsync()
		{
			return await this.Data.CountAsync(x => !(x.EmailConfirmed && x.PhoneNumberConfirmed)).AwaitBackground();
		}

		public async Task<List<Tuple<DateTime, int>>> CountByDay(DateTime start)
		{
			var query = this.Data.Where(x => x.RegisteredAt >= start)
				.GroupBy(x => x.RegisteredAt.Date)
				.Select(x => new Tuple<DateTime, int>(x.Key, x.Count()));
			return await query.ToListAsync().AwaitBackground();
		}

		public async Task<List<SensateUser>> GetMostRecentAsync(int number)
		{
			var query = this.Data.OrderByDescending(x => x.RegisteredAt);
			var ordered = query.Take(number);

			return await ordered.ToListAsync().AwaitBackground();
		}

		public async Task<bool> IsBanned(SensateUser user)
		{
			var raw = await this.GetRolesAsync(user).AwaitBackground();
			var roles = raw.ToArray();

			if(roles.Length == 0)
				return true;

			return this.IsInRole(roles, UserRoles.Banned);
		}

		public async Task<bool> IsAdministrator(SensateUser user)
		{
			return await this.IsInRole(user, UserRoles.Administrator);
		}

		public async Task<bool> ClearRolesForAsync(SensateUser user)
		{
			var roles = await this._manager.GetRolesAsync(user).AwaitBackground();

			if(roles.Count <= 0)
				return true;

			var result = await this._manager.RemoveFromRolesAsync(user, roles).AwaitBackground();
			return result.Succeeded;
		}

		public async Task<bool> AddToRolesAsync(SensateUser user, IEnumerable<string> roles)
		{
			var result = await this._manager.AddToRolesAsync(user, roles);
			return result.Succeeded;
		}

		private async Task<bool> IsInRole(SensateUser user, string role)
		{
			var raw = await this.GetRolesAsync(user).AwaitBackground();
			var roles = raw.Select(r => r.ToUpper());

			return roles.Contains(role.ToUpper());
		}

		private bool IsInRole(string[] roles, string role)
		{
			var _roles = roles.Select(r => r.ToUpper());
			return _roles.Contains(role.ToUpper());
		}
	}
}
