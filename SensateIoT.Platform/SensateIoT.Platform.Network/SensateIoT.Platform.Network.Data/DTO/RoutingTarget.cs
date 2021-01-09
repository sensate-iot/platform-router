/*
 * Routing information for a message, tied to a sensor.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Platform.Network.Data.DTO
{
	public class RoutingTarget
	{
		public RouteType Type { get; set; }
		public string Target { get; set; }
	}
}
