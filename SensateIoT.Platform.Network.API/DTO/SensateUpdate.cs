/*
 * Sensor update viewmodel.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Platform.Network.API.DTO
{
	public class SensorUpdate
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Secret { get; set; }
		public bool? StorageEnabled { get; set; }
	}
}
