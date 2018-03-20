/*
 * Abstract controller containing some generic
 * useful calls.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using Microsoft.AspNetCore.Mvc;

namespace SensateService.Controllers
{
	public abstract class AbstractController : Controller
	{
		public const int ServerFaultCode = 500;
		public const int ServerFaultBadGateway = 502;
		public const int ServerFaultUnavailable = 503;

		protected AbstractController() : base()
		{
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
	}
}
