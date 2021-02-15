/*
 * Async command model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateIoT.Platform.Network.Data.Enums;

namespace SensateIoT.Platform.Network.Data.DTO
{
	public class Command
	{
		public CommandType Cmd { get; set; }
		public string Arguments { get; set; }
	}
}