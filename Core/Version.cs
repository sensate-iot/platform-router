/*
 * Version string implementation.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;

namespace SensateService
{
	public static class Version
	{
		public static string VersionString => String.Format(
			"Sensate Core {0}.{1}.{2}",
			Major, Minor, PatchLevel
		);

		public const int Major = 0;
		public const int Minor = 1;
		public const int PatchLevel = 1;
	}
}
