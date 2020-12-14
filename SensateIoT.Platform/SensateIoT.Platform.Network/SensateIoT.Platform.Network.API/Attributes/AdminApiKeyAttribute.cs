/*
 * Attribute to verify user status.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using SensateIoT.Platform.Network.API.DTO;
using SensateIoT.Platform.Network.Data.Models;

using UserRoles = SensateIoT.Platform.Network.API.Constants.UserRoles;

namespace SensateIoT.Platform.Network.API.Attributes
{
	public class AdminApiKeyAttribute : ActionFilterAttribute
	{
		private static bool IsInRole(User user, string role)
		{
			return user.UserRoles.Contains(role);
		}

		public override void OnActionExecuting([NotNull] ActionExecutingContext context)
		{
			Response<string> response;
			var valid = false;

			base.OnActionExecuting(context);

			if(context?.HttpContext.Items["ApiKey"] is ApiKey key && !key.Revoked) {
				valid = !key.ReadOnly && IsInRole(key.User, UserRoles.Administrator.ToUpper(CultureInfo.InvariantCulture));
			}

			if(valid) {
				return;
			}

			response = new Response<string>();
			response.AddError("Invalid API-key.");

			context.Result = new BadRequestObjectResult(response);
		}
	}
}