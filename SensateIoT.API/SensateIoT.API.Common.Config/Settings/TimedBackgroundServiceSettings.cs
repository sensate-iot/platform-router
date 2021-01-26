/*
 * Settings for TimedBackgroundService.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

namespace SensateIoT.API.Common.Config.Settings
{
	public class TimedBackgroundServiceSettings
	{
		public int Interval { get; set; }
		public int StartDelay { get; set; }
	}
}