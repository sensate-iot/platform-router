/*
 * Command publishing service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;
using SensateIoT.API.Common.Data.Enums;

namespace SensateIoT.API.Common.Core.Services.Processing
{
	public interface ICommandPublisher
	{
		Task PublishCommand(CommandType cmd, string argument, bool retain = false);
	}
}