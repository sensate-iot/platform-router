/*
 * Attribute that ensures a valid modelstate when added to
 * a controller method.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using SensateIoT.Platform.Network.API.DTO;

namespace SensateIoT.Platform.Network.API.Attributes
{
	public class ValidateModelAttribute : ActionFilterAttribute
	{

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			Response<string> response;

			base.OnActionExecuting(context);

			if(context == null) {
				return;
			}

			if(context.ModelState.IsValid) {
				return;
			}

			response = new Response<string>();

			foreach(var error in context.ModelState.Values.SelectMany(modelState => modelState.Errors)) {
				response.AddError(error.ErrorMessage);
			}


			context.Result = new BadRequestObjectResult(response);
		}
	}
}
