/*
 * Routing client interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;

using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.TriggerService.Abstract
{
	public interface IRouterClient
	{
		Task RouteControlMessageAsync(ControlMessage msg, ControlMessageType type);
	}
}
