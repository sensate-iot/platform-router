/*
 * Command publishing service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */


using System.Threading;
using System.Threading.Tasks;

using SensateIoT.Platform.Network.Data.Enums;

namespace SensateIoT.Platform.Network.API.Abstract
{
	public interface ICommandPublisher
	{
		Task PublishCommandAsync(CommandType type, string argument, CancellationToken ct = default);
	}
}
