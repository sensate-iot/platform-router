/*
 * Version string implementation.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;

namespace SensateService
{
	public static class Version
	{
		public static string VersionString
		{
			get {
				return String.Format(
					"Sensate Service {0}.{1}.{2}",
					Major, Minor, PatchLevel
				);
			}
		}

		public const int Major = 0;
		public const int Minor = 0;
		public const int PatchLevel = 1;
	}
}
