/*
 * Attribute to verify user status.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SensateIoT.API.Common.Data.Dto.Json.Out;
using SensateIoT.API.Common.Data.Enums;
using SensateIoT.API.Common.IdentityData.Models;
using UserRoles = SensateIoT.API.Common.Core.Constants.UserRoles;

namespace SensateIoT.API.Common.ApiCore.Attributes
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
				valid = !key.ReadOnly && IsInRole(key.User, UserRoles.Administrator.ToUpper(CultureInfo.InvariantCulture));
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