/*
 * Gateway response message.
 *
 * @author Michel Megens.
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Platform.Network.API.DTO
{
	public class GatewayResponse
	{
		public string Message { get; set; }
		public int Queued { get; set; }
		public int Rejected { get; set; }
	}
}
