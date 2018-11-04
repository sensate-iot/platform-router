/**
 * Admin control API.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models.Json.In;
using SensateService.Models.Json.Out;

namespace SensateService.Auth.Controllers
{
	[AdministratorUser]
	[Produces("application/json")]
	[Route("[controller]")]
	public class AdminController : AbstractController 
	{
		public AdminController(IUserRepository users) : base(users)
		{
		}

		[HttpPost("find")]
		[AdministratorUser]
		[ProducesResponseType(typeof(List<User>), 200)]
		public async Task<IActionResult> Find([FromBody] SearchQuery query)
		{
			List<User> users;
			var result = await this._users.FindByEmailAsync(query.Query).AwaitSafely();

			users = result.Select(user => new User {
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName,
					PhoneNumber = user.PhoneNumber,
					Id = user.Id
				}).ToList();

			return new OkObjectResult(users);
		}
	}
}