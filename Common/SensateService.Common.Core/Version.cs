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
		public static string VersionString => $"Sensate IoT Core {Major}.{Minor}.{PatchLevel}";

		public const int Major = 0;
		public const int Minor = 5;
		public const int PatchLevel = 1;
	}
}
