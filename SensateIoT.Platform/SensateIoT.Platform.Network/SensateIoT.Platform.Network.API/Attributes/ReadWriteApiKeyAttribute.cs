/*
 * API user attribute.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.API.Attributes
{
	public class ReadWriteApiKeyAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var valid = false;

			base.OnActionExecuting(context);

			if(context?.HttpContext.Items["ApiKey"] is ApiKey key && !key.Revoked) {
				valid = !key.ReadOnly;
			}

			if(valid) {
				return;
			}

			context.Result = new ForbidResult();
		}
	}
}