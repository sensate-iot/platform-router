/*
 * Storage service router.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Prometheus;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Routing.Abstract;

namespace SensateIoT.Platform.Router.Common.Routing.Routers
{
	public class StorageRouter : IRouter
	{
		private readonly IRemoteStorageQueue m_storageQueue;
		private readonly Counter m_counter;

		public string Name => "Storage Router";

		public StorageRouter(IRemoteStorageQueue queue)
		{
			this.m_storageQueue = queue;
			this.m_counter = Metrics.CreateCounter("router_storage_messages_routed_total",
														   "Total number of routed storage messages.");
		}

		public bool Route(Sensor sensor, IPlatformMessage message, NetworkEvent networkEvent)
		{
			if(!sensor.StorageEnabled || message.Type == MessageType.ControlMessage) {
				return true;
			}

			this.m_counter.Inc();
			this.m_storageQueue.Enqueue(message);
			networkEvent.Actions.Add(NetworkEventType.MessageStorage);

			return true;
		}
	}
}
