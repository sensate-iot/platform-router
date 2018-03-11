/*
 * Extension methods for the IUrlHelper interface.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using Microsoft.AspNetCore.Mvc;
using SensateService.Controllers;

namespace SensateService.Helpers
{
    public static class UrlHelperExtensions
    {
		public static string EmailConfirmationLink(this IUrlHelper url, string id, string code, string scheme)
		{
			var action = url.Action(
				action: nameof(AccountsController.ConfirmEmail),
				controller: "accounts",
				values: new { id, code },
				protocol: scheme
				);

			return action;
		}
    }
}
