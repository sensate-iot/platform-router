/*
 * Message targeting a specific sensor device.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateService.Models;

namespace SensateService.NetworkApi.Models
{
	public class ControlMessage : Message
	{
		public ulong NodeAddress { get; set; }
	}
}