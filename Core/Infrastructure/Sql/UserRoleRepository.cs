/*
 * Identity role repository implementation.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SensateService.Exceptions;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class UserRoleRepository : AbstractSqlRepository<string, UserRole>, IUserRoleRepository
	{
		private readonly DbSet<IdentityUserRole<string>> _userRoles;
		private readonly IUserRepository _users;
		private readonly RoleManager<UserRole> _roles;

		public UserRoleRepository(SensateSqlContext context, IUserRepository urepo, RoleManager<UserRole> roles) :
			base(context)
		{
			this._users = urepo;
			this._userRoles = context.UserRoles;
			this._roles = roles;
		}

		public void Create(string name, string description)
		{
			var role = new UserRole() {
				Description = description,
				Name = name
			};
			this.Create(role);
		}

		public override void Create(UserRole obj)
		{
			var result = this._roles.CreateAsync(obj).Result;
			if(!result.Succeeded)
				throw new DatabaseException("Unable to create user role!");
		}

		public async Task CreateAsync(string name, string description)
		{
			var role = new UserRole() {
				Description = description,
				Name = name
			};
			await this.CreateAsync(role).AwaitSafely();
		}

		public override async Task CreateAsync(UserRole obj)
		{
			var result = await this._roles.CreateAsync(obj).AwaitSafely();
			if(!result.Succeeded)
				throw new DatabaseException("Unable to create user role!");
		}

		public override void Delete(string id)
		{
			var role = this.GetById(id);
			this._roles.DeleteAsync(role);
		}

		public override async Task DeleteAsync(string id)
		{
			await Task.Run(() => this.Delete(id)).AwaitSafely();
		}

		public override UserRole GetById(string id)
		{
			return (from role in this.Data
					where role.Id == id
					select role).Single<UserRole>();
		}

		public UserRole GetByName(string name)
		{
			return (from role in this.Data
					where role.Name == name
					select role).Single<UserRole>();
		}

		public IEnumerable<string> GetRolesFor(SensateUser user)
		{
			return this._users.GetRoles(user);
		}

		public async Task<IEnumerable<string>> GetRolesForAsync(SensateUser user)
		{
			return await this._users.GetRolesAsync(user).AwaitSafely();
		}

		public IEnumerable<SensateUser> GetUsers(string name)
		{
			IEnumerable<IdentityUserRole<string>> roles;
			List<SensateUser> users;
			var role = this.GetByName(name);

			roles = from r in this._userRoles
					where r.RoleId == role.Id
					select r;

			users = new List<SensateUser>();
			foreach(var r in roles) {
				var user = this._users.Get(r.UserId);
				users.Add(user);
			}

			return users;
		}

		public void Update(string name, UserRole role)
		{
			var obj = this.GetById(name);

			this.Data.Update(obj);
			if(role.Name != null)
				obj.Name = role.Name;

			if(role.Description != null)
				obj.Description = role.Description;

			this.Commit(obj);
		}

		public override void Update(UserRole obj)
		{
			this.Update(obj.Name, obj);
		}

		public async Task UpdateAsync(string name, UserRole obj)
		{
			await Task.Run(() => {
				this.Update(name, obj);
			}).AwaitSafely();
		}
	}
}
