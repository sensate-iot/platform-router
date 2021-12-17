/*
 * Storage service router.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.Extensions.Logging;
using Prometheus;
using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Routing.Abstract;
using SensateIoT.Platform.Router.Contracts.DTO;
using SensateIoT.Platform.Router.Data.Abstract;
using SensateIoT.Platform.Router.Data.DTO;

namespace SensateIoT.Platform.Router.Common.Routing.Routers
{
	public class StorageRouter : IRouter
	{
		private readonly IRemoteStorageQueue m_storageQueue;
		private readonly Counter m_counter;
		private readonly ILogger<StorageRouter> m_logger;

		public string Name => "Storage Router";

		public StorageRouter(IRemoteStorageQueue queue, ILogger<StorageRouter> logger)
		{
			this.m_storageQueue = queue;
			this.m_logger = logger;
			this.m_counter = Metrics.CreateCounter("router_storage_messages_routed_total",
														   "Total number of routed storage messages.");
		}

		public bool Route(Sensor sensor, IPlatformMessage message, NetworkEvent networkEvent)
		{
			if(!this.ShouldStoreMessage(sensor, message)) {
				return true;
			}

			this.m_counter.Inc();
			this.m_storageQueue.Enqueue(message);
			networkEvent.Actions.Add(NetworkEventType.MessageStorage);

			return true;
		}

		private bool ShouldStoreMessage(Sensor sensor, IPlatformMessage message)
		{
			if(!sensor.StorageEnabled) {
				this.m_logger.LogDebug("Skipping storage for {sensorId}: storage not enabled", sensor.ID.ToString());
				return false;
			}

			if(message.Type == MessageType.ControlMessage) {
				this.m_logger.LogDebug("Skipping storage for {sensorId}: message is of type Control Message", sensor.ID.ToString());
				return false;
			}

			return true;
		}
	}
}
