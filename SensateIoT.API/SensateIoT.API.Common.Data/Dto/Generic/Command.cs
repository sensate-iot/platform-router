/*
 * Command DTO class.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateIoT.API.Common.Data.Enums;

namespace SensateIoT.API.Common.Data.Dto.Generic
{
	public class Command
	{
		public CommandType Cmd { get; set; }
		public string Arguments { get; set; }
	}
}