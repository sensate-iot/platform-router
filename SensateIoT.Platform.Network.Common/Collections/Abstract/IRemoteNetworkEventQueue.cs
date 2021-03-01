/*
 * Remote price update queue interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System.Threading.Tasks;

namespace SensateIoT.Platform.Network.Common.Collections.Abstract
{
	public interface IRemoteNetworkEventQueue
	{
		void EnqueueEvent(NetworkEvent netEvent);
		Task FlushEventsAsync(CancellationToken ct = default);
	}
}
