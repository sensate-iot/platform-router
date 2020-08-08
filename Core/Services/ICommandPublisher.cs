/*
 * Command publishing service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;
using SensateService.Enums;

namespace SensateService.Services
{
	public interface ICommandPublisher
	{
		Task PublishCommand(AuthServiceCommand cmd, string argument, bool retain = false);
	}
}