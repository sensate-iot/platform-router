/*
 * Async command model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateIoT.Platform.Router.Data.Enums;

namespace SensateIoT.Platform.Router.Data.DTO
{
	public class Command
	{
		public CommandType Cmd { get; set; }
		public string Arguments { get; set; }
	}
}
