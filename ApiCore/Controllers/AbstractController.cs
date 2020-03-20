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

using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.Out;

namespace SensateService.ApiCore.Controllers
{
	public abstract class AbstractController : Controller
	{
		protected readonly IUserRepository _users;

		protected SensateUser CurrentUser { get; }

		protected AbstractController(IUserRepository users, IHttpContextAccessor ctx)
		{
			var uid = ctx.HttpContext.User;
			this._users = users;
			this.CurrentUser = null;

			if(uid != null) {
				this.CurrentUser = this._users.GetByClaimsPrinciple(uid);
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

			return await this._users.GetByClaimsPrincipleAsync(this.User);
		}

		protected IActionResult InvalidInputResult(string msg = "Invalid input!")
		{
			var status = new Status { Message = msg, ErrorCode = ReplyCode.BadInput };
			return new BadRequestObjectResult(status);
		}

		protected IActionResult NotFoundInputResult(string msg)
		{
			var status = new Status {
				Message = msg,
				ErrorCode = ReplyCode.NotFound
			};

			return new NotFoundObjectResult(status);
		}

		protected bool IsValidUri(string uri)
		{
			bool result;

			result = Uri.TryCreate(uri, UriKind.Absolute, out Uri resulturi) &&
					 (resulturi.Scheme == Uri.UriSchemeHttp || resulturi.Scheme == Uri.UriSchemeHttps);
			return result;
		}
	}
}
