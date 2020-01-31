/*
 * Control message model for actuators.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateService.Models
{
	public class ControlMessage
	{
        public long Id { get; set; }
        public int NodeId { get; set; }
        public string SensorId { get; set; }
        public string Data { get; set; }
	}
}