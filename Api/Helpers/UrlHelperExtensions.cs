﻿/*
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
		public static string EmailConfirmationLink(this IUrlHelper url, string id, string code, string scheme, string target = null)
		{
			object targetValues;

			if(String.IsNullOrEmpty(target)) {
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
				controller: "accounts",
				values: targetValues,
				protocol: scheme
				);

			return action;
		}

		public static string PasswordResetLink(this IUrlHelper url, string id, string code, string scheme)
		{
			var action = url.Action(
				action: nameof(AccountsController.Resetpassword),
				controller: "accounts",
				values: new { id, code },
				protocol: scheme
				);

			return action;
		}
    }
}
