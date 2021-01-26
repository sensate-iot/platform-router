/*
 * Identity role repository implementation.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SensateIoT.API.Common.Core.Exceptions;
using SensateIoT.API.Common.Core.Helpers;
using SensateIoT.API.Common.Core.Infrastructure.Repositories;
using SensateIoT.API.Common.IdentityData.Models;

namespace SensateIoT.API.Common.Core.Infrastructure.Sql
{
	public class UserRoleRepository : AbstractSqlRepository<SensateRole>, IUserRoleRepository
	{
		private readonly DbSet<SensateUserRole> _userRoles;
		private readonly IUserRepository _users;
		private readonly RoleManager<SensateRole> _roles;

		public UserRoleRepository(SensateSqlContext context, IUserRepository urepo, RoleManager<SensateRole> roles) :
			base(context)
		{
			this._users = urepo;
			this._userRoles = context.UserRoles;
			this._roles = roles;
		}

		public void Create(string name, string description)
		{
			var role = new SensateRole() {
				Description = description,
				Name = name
			};
			this.Create(role);
		}

		public override void Create(SensateRole obj)
		{
			var result = this._roles.CreateAsync(obj).Result;
			if(!result.Succeeded)
				throw new DatabaseException("Unable to create user role!");
		}

		public async Task CreateAsync(string name, string description)
		{
			var role = new SensateRole() {
				Description = description,
				Name = name
			};
			await this.CreateAsync(role).AwaitBackground();
		}

		public override async Task CreateAsync(SensateRole obj, CancellationToken ct = default)
		{
			var result = await this._roles.CreateAsync(obj).AwaitBackground();
			if(!result.Succeeded)
				throw new DatabaseException("Unable to create user role!");
		}

		public async Task<SensateRole> GetByNameAsync(string name)
		{
			return await this.Data.FirstOrDefaultAsync(role => role.Name == name).AwaitBackground();
		}

		public void Delete(string id)
		{
			var role = this.GetById(id);
			this._roles.DeleteAsync(role);
		}

		public async Task DeleteAsync(string id)
		{
			await Task.Run(() => this.Delete(id)).AwaitBackground();
		}

		public SensateRole GetById(string id)
		{
			return (from role in this.Data
					where role.Id == id
					select role).Single();
		}

		public SensateRole GetByName(string name)
		{
			return (from role in this.Data
					where role.Name == name
					select role).Single();
		}

		public IEnumerable<string> GetRolesFor(SensateUser user)
		{
			return this._users.GetRoles(user);
		}

		public async Task<IEnumerable<string>> GetRolesForAsync(SensateUser user)
		{
			return await this._users.GetRolesAsync(user).AwaitBackground();
		}

		public IEnumerable<SensateUser> GetUsers(string name)
		{
			IEnumerable<IdentityUserRole<string>> roles;
			var role = this.GetByName(name);

			roles = from r in this._userRoles
					where r.RoleId == role.Id
					select r;

			return roles.Select(r => this._users.Get(r.UserId)).ToList();
		}

		public void Update(string name, SensateRole role)
		{
			var obj = this.GetById(name);

			this.Data.Update(obj);
			if(role.Name != null)
				obj.Name = role.Name;

			if(role.Description != null)
				obj.Description = role.Description;

			this.Commit(obj);
		}

		public async Task UpdateAsync(string name, SensateRole obj)
		{
			await Task.Run(() => {
				this.Update(name, obj);
			}).AwaitBackground();
		}
	}
}
