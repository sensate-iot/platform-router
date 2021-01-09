/*
 * Message to send to an actuator.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateIoT.Platform.Network.API.DTO
{
	public class ActuatorMessage
	{
		[Required, StringLength(24)]
		public string SensorId { get; set; }
		[Required, MaxLength(4096)]
		public string Data { get; set; }
	}
}
