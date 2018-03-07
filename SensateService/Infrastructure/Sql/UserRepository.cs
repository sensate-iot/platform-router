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
using System.Threading;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;

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

		public override void Create(SensateUser obj)
		{
			throw new SystemException("UserRepository.Create is forbidden!");
		}

		public override bool Delete(string id)
		{
			var obj = this.Get(id);

			if(obj == null)
				return false;

			this.Data.Remove(obj);
			this.Commit(obj);
			return true;
		}

		public override SensateUser GetById(string id)
		{
			return this.Data.FirstOrDefault(x => x.Id == id);
		}

		public SensateUser GetByEmail(string email)
		{
			return this.Data.FirstOrDefault(x => x.Email == email);
		}

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
			var user = await Task.Run(() => {
				return this.GetById(key);
			});

			return user;
		}

		public override bool Replace(SensateUser obj1, SensateUser obj2)
		{
			return false;
		}

		public override bool Update(SensateUser obj)
		{
			var orig = this.GetByEmail(obj.Email);

			this.Data.Update(obj);
			this.Commit(obj);
			return true;
		}

		public SensateUser GetCurrentUser(ClaimsPrincipal cp)
		{
			string email;

			email = this._manager.GetUserId(cp);
			return this.GetByEmail(email);
		}

		public async Task<SensateUser> GetCurrentUserAsync(ClaimsPrincipal cp)
		{
			string email;

			email = this._manager.GetUserId(cp);
			return await this.GetByEmailAsync(email);
		}
	}
}
