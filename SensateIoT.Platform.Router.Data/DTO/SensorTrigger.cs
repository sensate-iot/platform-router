/*
 * Sensor trigger information.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Platform.Router.Data.DTO
{
	public struct SensorTrigger
	{
		public bool HasActions { get; set; }
		public bool IsTextTrigger { get; set; }
	}
}
