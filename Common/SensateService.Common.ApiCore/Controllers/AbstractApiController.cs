/*
 * Abstract API controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SensateService.Common.IdentityData.Models;

namespace SensateService.ApiCore.Controllers
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

