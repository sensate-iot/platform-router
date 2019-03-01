/*
 * User repository interface.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

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

		public virtual void Delete(string id)
		{
			var obj = this.Get(id);

			if(obj == null)
				return;

			this.Data.Remove(obj);
			this.Commit(obj);
		}

		public virtual SensateUser GetById(string id) => String.IsNullOrEmpty(id) ? null : this.Data.FirstOrDefault(x => x.Id == id);
		public SensateUser GetByEmail(string email) => this.Data.FirstOrDefault(x => x.Email == email);

		public async Task<SensateUser> GetByEmailAsync(string email)
		{
			return await Task.Run(() => {
				return this.Data.FirstOrDefault(x => x.Email == email);
			});
		}

		public virtual SensateUser Get(string key)
		{
			return this.GetById(key);
		}

		public virtual async Task<SensateUser> GetAsync(string key)
		{
			var user = await Task.Run(() => this.GetById(key));
			return user;
		}

		public override Task CreateAsync(SensateUser obj)
		{
			throw new SystemException("UserRepository.CreateAsync is forbidden!");
		}

		public virtual async Task DeleteAsync(string id)
		{
			var obj = await this.GetAsync(id);

			if(obj == null)
				return;

			this.Data.Remove(obj);
			await this.CommitAsync();
		}

		public SensateUser GetByClaimsPrinciple(ClaimsPrincipal cp)
		{
			string email;

			email = this._manager.GetUserId(cp);
			return this.GetByEmail(email);
		}

		public async Task<SensateUser> GetByClaimsPrincipleAsync(ClaimsPrincipal cp)
		{
			string email;

			email = this._manager.GetUserId(cp);
			return await this.GetByEmailAsync(email);
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

		public async Task<IEnumerable<SensateUser>> FindByEmailAsync(string email)
		{
			var result = this.Data.Where(x => x.Email.Contains(email));
			return await result.ToListAsync().AwaitBackground();
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
				.Select( x => new Tuple<DateTime, int>(x.Key, x.Count()) );
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
			return await this.IsInRole(user, UserRoles.Banned);
		}

		public async Task<bool> IsAdministrator(SensateUser user)
		{
			return await this.IsInRole(user, UserRoles.Administrator);
		}

		public async Task<bool> ClearRolesForAsync(SensateUser user)
		{
			var roles = await this._manager.GetRolesAsync(user);
			var result = await this._manager.RemoveFromRolesAsync(user, roles);

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
	}
}
