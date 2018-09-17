/*
 * Extension methods for the IHostingEnvironment interface.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace SensateService.Helpers
{
	public static class HostingHelper
	{
		public static string GetTemplatePath(this IHostingEnvironment environment, string template)
		{
			var root = environment.ContentRootPath;

			root += Path.DirectorySeparatorChar;
			root += "Templates" + Path.DirectorySeparatorChar.ToString() + template;
			return root;
		}
	}
}
