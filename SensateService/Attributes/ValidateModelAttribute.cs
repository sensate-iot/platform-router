/*
 * Attribute that ensures a valid modelstate when added to
 * a controller method.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using SensateService.Models.Json.Out;

namespace SensateService
{
	public class ValidateModelAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			Status status;

			base.OnActionExecuting(context);

			if(!context.ModelState.IsValid) {
				status = new Status {
					ErrorCode = 400,
					Message = "Invalid request"
				};

				context.Result = new BadRequestObjectResult(status);
			}
		}
	}
}
