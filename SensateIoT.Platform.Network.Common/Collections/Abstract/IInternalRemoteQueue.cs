/*
 * Remote queue interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.Common.Collections.Abstract
{
	public interface IInternalRemoteQueue
	{
		void EnqueueToMessageTriggerService(IPlatformMessage message);
		void EnqueueMeasurementToTriggerService(IPlatformMessage message);
		void EnqueueMeasurementToTarget(IPlatformMessage message, RoutingTarget target);
		void EnqueueMessageToTarget(IPlatformMessage message, RoutingTarget target);
		void EnqueueControlMessageToTarget(IPlatformMessage message, RoutingTarget target);
		Task FlushAsync();
		Task FlushLiveDataAsync();

		void SyncLiveDataHandlers(IEnumerable<LiveDataHandler> handlers);
	}
}
