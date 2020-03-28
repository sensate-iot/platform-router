/*
 * Admin control API.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models.Json.In;
using SensateService.Models.Json.Out;

namespace SensateService.AuthApi.Controllers
{
	[AdministratorUser]
	[Produces("application/json")]
	[Route("auth/v1/[controller]")]
	public class AdminController : AbstractController
	{
		private readonly ILogger<AdminController> m_logger;

		public AdminController(IUserRepository users, IHttpContextAccessor ctx, ILogger<AdminController> logger) : base(users, ctx)
		{
			this.m_logger = logger;
		}

		[HttpPost("find-users")]
		[ProducesResponseType(typeof(PaginationResult<User>), 200)]
		[ValidateModel]
		public async Task<IActionResult> Find([FromBody] SearchQuery query, [FromQuery] int skip = 0, [FromQuery] int limit = 0, [FromQuery] bool count = false)
		{
			PaginationResult<User> rv;
			var result = await this._users.FindByEmailAsync(query.Query, skip, limit).AwaitBackground();

			var users = result.Select(user => {
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

			rv = new PaginationResult<User> {
				Count = await this._users.CountFindAsync(query.Query).AwaitBackground(),
				Values = users
			};

			return new OkObjectResult(rv);
		}

		[HttpGet("users")]
		[ProducesResponseType(typeof(PaginationResult<User>), 200)]
		public async Task<IActionResult> GetMostRecentUsers(int skip = 0, int limit = 10)
		{
			List<User> users;
			var rv = new PaginationResult<User>();

			try {
				var userWorker = this._users.GetMostRecentAsync(skip, limit);
				var query = await userWorker.AwaitBackground();

				users = query.Values.Select(user => new User {
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName,
					PhoneNumber = user.PhoneNumber,
					Id = user.Id,
					RegisteredAt = user.RegisteredAt.ToUniversalTime(),
					Roles = this._users.GetRoles(user)
				}).ToList();

				rv.Values = users;
				rv.Count = query.Count;
			} catch(Exception ex) {
				this.m_logger.LogInformation(ex, "Unable to fetch recent users!");

				return this.BadRequest(new Status {
					Message = "Unable to fetch recent users!",
					ErrorCode = ReplyCode.UnknownError
				});
			}


			return this.Ok(rv);
		}
	}
}
