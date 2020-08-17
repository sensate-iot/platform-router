/*
 * Abstract controller containing some generic
 * useful calls.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SensateService.Common.Data.Enums;
using SensateService.Common.IdentityData.Models;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models.Json.Out;

namespace SensateService.ApiCore.Controllers
{
	public abstract class AbstractController : Controller
	{
		protected readonly IUserRepository _users;

		protected SensateUser CurrentUser { get; }

		protected AbstractController(IUserRepository users, IHttpContextAccessor ctx)
		{
			this._users = users;

			if(this.CurrentUser == null) {
				this.CurrentUser = ctx?.HttpContext.Items["UserData"] as SensateUser;
			}
		}

		protected StatusCodeResult ServerFault()
		{
			return this.StatusCode(ErrorCode.ServerFaultGeneric.ToInt());
		}

		protected StatusCodeResult BadGateway()
		{
			return this.StatusCode(ErrorCode.ServerFaultBadGateway.ToInt());
		}

		protected StatusCodeResult ServiceUnavailable()
		{
			return this.StatusCode(ErrorCode.ServerFaultUnavailable.ToInt());
		}

		protected async Task<SensateUser> GetCurrentUserAsync()
		{
			if(this.User == null)
				return null;

			return await this._users.GetByClaimsPrincipleAsync(this.User).AwaitBackground();
		}

		protected static IActionResult InvalidInputResult(string msg = "Invalid input!")
		{
			var status = new Status { Message = msg, ErrorCode = ReplyCode.BadInput };
			return new BadRequestObjectResult(status);
		}

		protected static IActionResult NotFoundInputResult(string msg)
		{
			var status = new Status {
				Message = msg,
				ErrorCode = ReplyCode.NotFound
			};

			return new NotFoundObjectResult(status);
		}
	}
}
