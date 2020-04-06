/*
 * Attribute to verify user status.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using SensateService.Models;

namespace SensateService.ApiCore.Attributes
{
	public class AdminApiKeyAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var valid = false;

			base.OnActionExecuting(context);

			if(context.HttpContext.Items["ApiKey"] is SensateApiKey key && !key.Revoked) {
				valid = !key.ReadOnly;
			}

			if(valid) {
				return;
			}

			context.Result = new ForbidResult();
		}
	}
}