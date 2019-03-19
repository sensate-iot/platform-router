/*
 * API user attribute.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SensateService.Enums;
using SensateService.Models;
using SensateService.Models.Json.Out;

namespace SensateService.ApiCore.Attributes
{
	public class ReadWriteApiKeyAttribute : ActionFilterAttribute 
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			Status status;
			var valid = false;

			base.OnActionExecuting(context);

			if(context.HttpContext.Items["ApiKey"] is SensateApiKey key && !key.Revoked) {
				valid = !key.ReadOnly;
			}

			if(valid)
				return;

			status = new Status {
				ErrorCode = ReplyCode.NotAllowed,
				Message = "Invalid API key"
			};

			context.Result = new BadRequestObjectResult(status);
		}
	}
}