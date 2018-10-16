/*
 * Extension methods for the IUrlHelper interface.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using Microsoft.AspNetCore.Mvc;

using SensateService.Api.Controllers.V1;

namespace SensateService.Api.Helpers
{
    public static class UrlHelperExtensions
    {
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
