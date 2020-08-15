/*
 * Abstract API controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SensateService.Models;

namespace SensateService.ApiCore.Controllers
{
	public class AbstractApiController : Controller
	{
		public SensateUser CurrentUser { get; }
		public SensateApiKey ApiKey { get; }

		public AbstractApiController(IHttpContextAccessor ctx)
		{
			var key = ctx.HttpContext.Items["ApiKey"] as SensateApiKey;

			this.ApiKey = key;
			this.CurrentUser = key?.User;
		}
	}
}

