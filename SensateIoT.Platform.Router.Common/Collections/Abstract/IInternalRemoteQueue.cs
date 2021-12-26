/*
 * Remote queue interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading.Tasks;

using SensateIoT.Platform.Router.Data.Abstract;
using SensateIoT.Platform.Router.Data.DTO;
using SensateIoT.Platform.Router.Data.Models;

namespace SensateIoT.Platform.Router.Common.Collections.Abstract
{
	public interface IInternalRemoteQueue
	{
		public int Count { get; }

		void EnqueueMeasurementToTarget(IPlatformMessage message, RoutingTarget target);
		void EnqueueMessageToTarget(IPlatformMessage message, RoutingTarget target);
		void EnqueueControlMessageToTarget(IPlatformMessage message, RoutingTarget target);
		Task FlushAsync();

		void SyncLiveDataHandlers(IEnumerable<LiveDataHandler> handlers);
	}
}
