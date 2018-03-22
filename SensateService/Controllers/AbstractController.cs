/*
 * Abstract controller containing some generic
 * useful calls.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.Out;

namespace SensateService.Controllers
{
	public abstract class AbstractController : Controller
	{
		public const int ServerFaultCode = 500;
		public const int ServerFaultBadGateway = 502;
		public const int ServerFaultUnavailable = 503;

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
			return this.StatusCode(ServerFaultCode);
		}

		protected StatusCodeResult BadGateway()
		{
			return this.StatusCode(ServerFaultBadGateway);
		}

		protected StatusCodeResult ServiceUnavailable()
		{
			return this.StatusCode(ServerFaultUnavailable);
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
			status.ErrorCode = 400;

			return new BadRequestObjectResult(status);
		}
	}
}
