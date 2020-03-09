/**
 * Admin control API.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models.Json.In;
using SensateService.Models.Json.Out;

namespace SensateService.AuthApi.Controllers
{
	[AdministratorUser]
	[Produces("application/json")]
	[Route("[controller]")]
	public class AdminController : AbstractController
	{
		public AdminController(IUserRepository users, IHttpContextAccessor ctx) : base(users, ctx)
		{
		}

		[HttpPost("find-users")]
		[ProducesResponseType(typeof(List<User>), 200)]
		[ValidateModel]
		public async Task<IActionResult> Find([FromBody] SearchQuery query)
		{
			List<User> users;
			var result = await this._users.FindByEmailAsync(query.Query).AwaitBackground();

			users = result.Select(user => {
				var roles = this._users.GetRoles(user);

				return new User {
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName,
					PhoneNumber = user.PhoneNumber,
					Id = user.Id,
					RegisteredAt = user.RegisteredAt.ToUniversalTime(),
					Roles = roles
				};
			}).ToList();

			return new OkObjectResult(users);
		}

		[HttpGet("users")]
		[ProducesResponseType(typeof(List<User>), 200)]
		public async Task<IActionResult> GetMostRecentUsers()
		{
			List<User> users;

			var userWorker = this._users.GetMostRecentAsync(10);
			var query = await userWorker.AwaitBackground();

			users = query.Select(user => new User {
				Email = user.Email,
				FirstName = user.FirstName,
				LastName = user.LastName,
				PhoneNumber = user.PhoneNumber,
				Id = user.Id,
				RegisteredAt = user.RegisteredAt.ToUniversalTime(),
				Roles = this._users.GetRoles(user)
			}).ToList();

			return this.Ok(users);
		}
	}
}
