/*
 * Sensor update viewmodel.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateService.NetworkApi.Models
{
	public class SensorUpdate
	{
		public string Name { get; set; }
        public string Description { get; set; }
        public string Secret { get; set; }
	}
}