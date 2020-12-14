/*
 * Abstract API controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.API.Controllers
{
	public class AbstractApiController : Controller
	{
		public User CurrentUser { get; }
		public ApiKey ApiKey { get; }

		public AbstractApiController(IHttpContextAccessor ctx)
		{
			if(ctx == null) {
				return;
			}

			var key = ctx.HttpContext.Items["ApiKey"] as ApiKey;

			this.ApiKey = key;
			this.CurrentUser = key?.User;
		}
	}
}

