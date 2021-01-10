/*
 * Abstract API controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SensateIoT.API.Common.IdentityData.Models;

namespace SensateIoT.API.Common.ApiCore.Controllers
{
	public class AbstractApiController : Controller
	{
		public SensateUser CurrentUser { get; }
		public SensateApiKey ApiKey { get; }

		public AbstractApiController(IHttpContextAccessor ctx)
		{
			if(ctx == null) {
				return;
			}

			var key = ctx.HttpContext.Items["ApiKey"] as SensateApiKey;

			this.ApiKey = key;
			this.CurrentUser = key?.User;
		}
	}
}

