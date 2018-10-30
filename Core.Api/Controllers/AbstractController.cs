/*
 * Abstract controller containing some generic
 * useful calls.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.Out;

namespace SensateService.Core.Api.Controllers
{
	public abstract class AbstractController : Controller
	{
		protected readonly IUserRepository _users;

		public SensateUser CurrentUser => this.User == null ? null : this._users.GetByClaimsPrinciple(this.User);

		protected AbstractController(IUserRepository users)
		{
			this._users = users;
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
			if(base.User == null)
				return null;

			return await this._users.GetByClaimsPrincipleAsync(base.User);
		}

		protected IActionResult InvalidInputResult(string msg = "Invalid input!")
		{
			var status = new Status {Message = msg, ErrorCode = ReplyCode.BadInput};
			return new BadRequestObjectResult(status);
		}

		protected IActionResult NotFoundInputResult(string msg)
		{
			var status = new Status();

			status.Message = msg;
			status.ErrorCode = ReplyCode.NotFound;

			return new NotFoundObjectResult(status);
		}

		protected string GetCurrentRoute()
		{
			if(!this.RouteData.Values.TryGetValue("controller", out object controller))
				return null;

			return !this.RouteData.Values.TryGetValue("action", out object action) ? null :
				String.Format("/{0}#{1}", controller.ToString(), action.ToString());
		}

		protected IPAddress GetRemoteAddress()
		{
			return this.HttpContext.Connection.RemoteIpAddress;
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
