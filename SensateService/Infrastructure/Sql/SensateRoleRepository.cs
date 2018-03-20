/*
 * Identity role repository implementation.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SensateService.Exceptions;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class SensateRoleRepository : AbstractSqlRepository<string, SensateRole>, ISensateRoleRepository
	{
		private readonly DbSet<IdentityUserRole<string>> _userRoles;
		private readonly IUserRepository _users;
		private readonly RoleManager<SensateRole> _roles;

		public SensateRoleRepository(SensateSqlContext context, IUserRepository urepo, RoleManager<SensateRole> roles) :
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
			await this.CreateAsync(role);
		}

		public override async Task CreateAsync(SensateRole obj)
		{
			var result = await this._roles.CreateAsync(obj);
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
			await Task.Run(() => this.Delete(id));
		}

		public override SensateRole GetById(string id)
		{
			return (from role in this.Data
					where role.Id == id
					select role).Single<SensateRole>();
		}

		public SensateRole GetByName(string name)
		{
			return (from role in this.Data
					where role.Name == name
					select role).Single<SensateRole>();
		}

		public IEnumerable<string> GetRolesFor(SensateUser user)
		{
			return this._users.GetRoles(user);
		}

		public async Task<IEnumerable<string>> GetRolesForAsync(SensateUser user)
		{
			return await this._users.GetRolesAsync(user);
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

		public override void Update(SensateRole obj)
		{
			this.Update(obj.Name, obj);
		}

		public async Task UpdateAsync(string name, SensateRole obj)
		{
			await Task.Run(() => {
				this.Update(name, obj);
			});
		}
	}
}
