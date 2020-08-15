/*
 * Attribute that ensures a valid modelstate when added to
 * a controller method.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using SensateService.Enums;
using SensateService.Models.Json.Out;

namespace SensateService.ApiCore.Attributes
{
	public class ValidateModelAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			Status status;

			base.OnActionExecuting(context);

			if(!context.ModelState.IsValid) {
				status = new Status {
					ErrorCode = ReplyCode.BadInput,
					Message = "Invalid request"
				};

				context.Result = new BadRequestObjectResult(status);
			}
		}
	}
}
