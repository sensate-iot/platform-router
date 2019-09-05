/*
 * Extension methods for the IUrlHelper interface.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using SensateService.AuthApi.Controllers;

namespace SensateService.AuthApi.Helpers
{
    public static class UrlHelperExtensions
    {
		[SuppressMessage("ReSharper", "RedundantAnonymousTypePropertyName")]
		public static string EmailConfirmationLink(this IUrlHelper url, string id, string code, string scheme, string host, string target = null)
		{
			object targetValues;

			if(string.IsNullOrEmpty(target)) {
				targetValues = new {
					id,
					code,
				};
			} else {
				targetValues = new {
					id,
					code,
					target = target
				};
			}

			var action = url.Action(
				action: nameof(AccountsController.ConfirmEmail),
				controller: "Accounts",
				values: targetValues,
				protocol: scheme,
				host: host
			);

			return action;
		}
    }
}
