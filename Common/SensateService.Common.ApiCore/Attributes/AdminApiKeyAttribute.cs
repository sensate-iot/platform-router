/*
 * Attribute to verify user status.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SensateService.Common.Data.Dto.Json.Out;
using SensateService.Common.Data.Enums;
using SensateService.Common.IdentityData.Models;
using UserRoles = SensateService.Constants.UserRoles;

namespace SensateService.ApiCore.Attributes
{
	public class AdminApiKeyAttribute : ActionFilterAttribute
	{
		private static bool IsInRole(SensateUser user, string role)
		{
			var roles = user.UserRoles.Select(x => x.Role.NormalizedName);
			return roles.Contains(role);
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			Status status;
			var valid = false;

			base.OnActionExecuting(context);

			if(context?.HttpContext.Items["ApiKey"] is SensateApiKey key && !key.Revoked) {
				valid = !key.ReadOnly && IsInRole(key.User, UserRoles.Administrator);
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