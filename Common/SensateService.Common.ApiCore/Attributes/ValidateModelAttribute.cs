/*
 * Attribute that ensures a valid modelstate when added to
 * a controller method.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SensateService.Common.Data.Dto.Json.Out;
using SensateService.Common.Data.Enums;
using SensateService.Constants;

namespace SensateService.ApiCore.Attributes
{
	public class ValidateModelAttribute : ActionFilterAttribute
	{

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			Status status;

			base.OnActionExecuting(context);
			status = new Status {
				ErrorCode = ReplyCode.BadInput,
				Message = HttpStatusMessages.InvalidRequestReply
			};

			if(context == null) {
				return;
			}

			if(context.ModelState.IsValid) {
				return;
			}

			context.Result = new BadRequestObjectResult(status);
		}
	}
}
