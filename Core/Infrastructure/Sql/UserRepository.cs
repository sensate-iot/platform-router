/*
 * User repository interface.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class UserRepository : AbstractSqlRepository<string, SensateUser>, IUserRepository
	{
		private readonly UserManager<SensateUser> _manager;

		public UserRepository(SensateSqlContext context, UserManager<SensateUser> manager) : base(context)
		{
			this._manager = manager;
		}

		public override void Create(SensateUser obj) => throw new SystemException("UserRepository.Create is forbidden!");

		public override void Delete(string id)
		{
			var obj = this.Get(id);

			if(obj == null)
				return;

			this.Data.Remove(obj);
			this.Commit(obj);
		}

		public override SensateUser GetById(string id) =>
			String.IsNullOrEmpty(id) ? null : this.Data.FirstOrDefault(x => x.Id == id);

		public SensateUser GetByEmail(string email) => this.Data.FirstOrDefault(x => x.Email == email);

		public async Task<SensateUser> GetByEmailAsync(string email)
		{
			return await Task.Run(() => {
				return this.Data.FirstOrDefault(x => x.Email == email);
			});
		}

		public SensateUser Get(string key)
		{
			return this.GetById(key);
		}

		public async Task<SensateUser> GetAsync(string key)
		{
			var user = await Task.Run(() => this.GetById(key));
			return user;
		}

		public override void Update(SensateUser obj)
		{
			this.Data.Update(obj);
			this.Commit(obj);
		}

		public override Task CreateAsync(SensateUser obj)
		{
			throw new SystemException("UserRepository.CreateAsync is forbidden!");
		}

		public override async Task DeleteAsync(string id)
		{
			var obj = this.Get(id);

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

			result.Wait();
			return result.Result;
		}

		public async Task<IEnumerable<string>> GetRolesAsync(SensateUser user)
		{
			return await this._manager.GetRolesAsync(user).AwaitSafely();
		}

		public async Task<IEnumerable<SensateUser>> FindByEmailAsync(string email)
		{
			var result = this.Data.Where(x => x.Email.Contains(email));
			return await result.ToListAsync().AwaitSafely();
		}

		public async Task<int> CountAsync()
		{
			return await this.Data.CountAsync().AwaitSafely();
		}

		public async Task<int> CountGhostUsersAsync()
		{
			return await this.Data.CountAsync(x => !(x.EmailConfirmed && x.PhoneNumberConfirmed)).AwaitSafely();
		}

		public async Task<List<Tuple<DateTime, int>>> CountByDay(DateTime start)
		{
			var query = this.Data.Where(x => x.RegisteredAt >= start)
				.GroupBy(x => x.RegisteredAt.Date)
				.Select( x => new Tuple<DateTime, int>(x.Key, x.Count()) );
			return await query.ToListAsync().AwaitSafely();
		}

		public async Task<List<SensateUser>> GetMostRecentAsync(int number)
		{
			var query = this.Data.OrderByDescending(x => x.RegisteredAt);
			var ordered = query.Take(number);

			return await ordered.ToListAsync().AwaitSafely();
		}
	}
}
