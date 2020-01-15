/*
 * Extension methods for the IHostingEnvironment interface.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace SensateService.AuthApi.Helpers
{
	public static class HostingHelper
	{
		public static string GetTemplatePath(this IWebHostEnvironment environment, string template)
		{
			var root = environment.ContentRootPath;

			root += Path.DirectorySeparatorChar;
			root += "Templates" + Path.DirectorySeparatorChar.ToString() + template;
			return root;
		}
	}
}
