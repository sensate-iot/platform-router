/*
 * Version string implementation.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateIoT.API.Common.Core
{
	public static class Version
	{
		public static string VersionString => $"Sensate IoT Core {Major}.{Minor}.{PatchLevel}";

		public const int Major = 1;
		public const int Minor = 0;
		public const int PatchLevel = 0;
	}
}
