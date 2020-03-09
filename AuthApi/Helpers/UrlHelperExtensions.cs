/*
 * Extension methods for the IUrlHelper interface.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using Microsoft.AspNetCore.Mvc;

namespace SensateService.AuthApi.Helpers
{
	public static class UrlHelperExtensions
	{
		public static string EmailConfirmationLink(this IUrlHelper url, string id, string code, string scheme, string host)
		{
			return $"{scheme}://{host}/{id}/{code}";
		}
	}
}
