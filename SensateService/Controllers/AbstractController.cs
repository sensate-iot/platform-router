/*
 * Abstract controller containing some generic
 * useful calls.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Threading.Tasks;
using System.Net;

using Microsoft.AspNetCore.Mvc;

using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.Out;
using SensateService.Enums;

namespace SensateService.Controllers
{
	public abstract class AbstractController : Controller
	{
		protected readonly IUserRepository _users;

		public SensateUser CurrentUser {
			get {
				if(this.User == null)
					return null;

				return this._users.GetByClaimsPrinciple(this.User);
			}
		}

		protected AbstractController(IUserRepository users) : base()
		{
			this._users = users;
		}

		protected StatusCodeResult ServerFault()
		{
			return this.StatusCode(Error.ServerFaultGeneric);
		}

		protected StatusCodeResult BadGateway()
		{
			return this.StatusCode(Error.ServerFaultBadGateway);
		}

		protected StatusCodeResult ServiceUnavailable()
		{
			return this.StatusCode(Error.ServerFaultUnavailable);
		}

		protected async Task<SensateUser> GetCurrentUserAsync()
		{
			if(base.User == null)
				return null;

			return await this._users.GetByClaimsPrincipleAsync(base.User);
		}

		protected IActionResult InvalidInputResult()
		{
			return this.InvalidInputResult("Invalid input!");
		}

		protected IActionResult InvalidInputResult(string msg)
		{
			var status = new Status();

			status.Message = msg;
			status.ErrorCode = ReplyCode.BadInput;

			return new BadRequestObjectResult(status);
		}

		protected IActionResult NotFoundInputResult(string msg)
		{
			var status = new Status();

			status.Message = msg;
			status.ErrorCode = ReplyCode.NotFound;

			return new NotFoundObjectResult(status);
		}

		protected string CurrentRoute()
		{
			object controller, action;

			if(!this.RouteData.Values.TryGetValue("controller", out controller))
				return null;

			if(!this.RouteData.Values.TryGetValue("action", out action))
				return null;

			return String.Format("{0}#{1}", controller.ToString(), action.ToString());
		}

		protected IPAddress GetRemoteAddress()
		{
			return this.HttpContext.Connection.RemoteIpAddress;
		}
	}
}
