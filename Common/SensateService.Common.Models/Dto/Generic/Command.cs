/*
 * Command DTO class.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateService.Common.Data.Enums;

namespace SensateService.Common.Data.Dto.Generic
{
	public class Command
	{
		public CommandType Cmd { get; set; }
		public string Arguments { get; set; }
	}
}