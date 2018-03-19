/*
 * Database seed for the default user roles.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Threading.Tasks;
using System.Linq;

using SensateService.Models;
using SensateService.Infrastructure.Sql;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace SensateService.Init
{
	public static class UserRoleSeed
	{
		public async static Task Initialize(SensateSqlContext ctx,
			RoleManager<SensateRole> roles, UserManager<SensateUser> manager)
		{
			SensateUser user;
			ctx.Database.EnsureCreated();
			IEnumerable<string> adminroles = new List<string> {
				"Administrators", "Users"
			};

			if(ctx.Roles.Any() || ctx.Users.Any() || ctx.UserRoles.Any())
				return;

			var uroles = new SensateRole[] {
				new SensateRole {
					Name = "Administrators",
					Description = "System administrators",
				},

				new SensateRole {
					Name = "Users",
					Description = "Normal users"
				},

				new SensateRole {
					Name = "Banned",
					Description = "Banned users"
				}
			};

			foreach(var role in uroles) {
				await roles.CreateAsync(role);
			}

			user = new SensateUser {
				Email = "root@example.com",
				FirstName = "System",
				LastName = "Administrator",
				PhoneNumber = "0612345678",
			};

			user.UserName = user.Email;
			user.EmailConfirmed = true;
			await manager.CreateAsync(user, "Root1234#xD");
			ctx.SaveChanges();
			user = await manager.FindByEmailAsync("root@example.com");
			await manager.AddToRolesAsync(user, adminroles);
		}
	}
}
