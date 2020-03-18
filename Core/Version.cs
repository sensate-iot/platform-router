/*
 * Version string implementation.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateService
{
	public static class Version
	{
		public static string VersionString => $"Sensate Core {Major}.{Minor}.{PatchLevel}";

		public const int Major = 0;
		public const int Minor = 4;
		public const int PatchLevel = 2;
	}
}
