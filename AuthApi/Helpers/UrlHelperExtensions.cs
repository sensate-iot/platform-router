/*
 * Extension methods for the IUrlHelper interface.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using SensateService.AuthApi.Controllers;

namespace SensateService.AuthApi.Helpers
{
    public static class UrlHelperExtensions
    {
		private static string TokenIdentifier = "{token}";

		[SuppressMessage("ReSharper", "RedundantAnonymousTypePropertyName")]
		public static string EmailConfirmationLink(this IUrlHelper url, string id, string code, string scheme, string host)
		{
			return $"{scheme}://{host}/{id}/{code}";
		}
    }
}
