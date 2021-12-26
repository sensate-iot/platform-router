/*
 * Live data router.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Linq;

using Microsoft.Extensions.Logging;
using Prometheus;
using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Exceptions;
using SensateIoT.Platform.Router.Common.Routing.Abstract;
using SensateIoT.Platform.Router.Contracts.DTO;
using SensateIoT.Platform.Router.Data.Abstract;
using SensateIoT.Platform.Router.Data.DTO;

namespace SensateIoT.Platform.Router.Common.Routing.Routers
{
	public class LiveDataRouter : IRouter
	{
		private readonly ILogger<LiveDataRouter> m_logger;
		private readonly IRemoteLiveDataQueue m_internalQueue;
		private readonly Counter m_liveDataCounter;

		public string Name => "Live Data Router";

		public LiveDataRouter(ILogger<LiveDataRouter> logger, IRemoteLiveDataQueue queue)
		{
			this.m_internalQueue = queue;
			this.m_logger = logger;
			this.m_liveDataCounter = Metrics.CreateCounter("router_livedata_messages_routed_total",
														   "Total number of routed live data messages.");
		}

		public bool Route(Sensor sensor, IPlatformMessage message, NetworkEvent networkEvent)
		{
			if(sensor.LiveDataRouting == null || sensor.LiveDataRouting?.Count <= 0) {
				return true;
			}

			switch(message.Type) {
			case MessageType.Measurement:
			case MessageType.Message:
			case MessageType.ControlMessage:
				return this.ProcessMessage(sensor, message, networkEvent);

			default:
				throw new RouterException(this.Name, $"unable to route message of type {message.Type:G}");
			}
		}

		private bool ProcessMessage(Sensor sensor, IPlatformMessage message, NetworkEvent evt)
		{
			this.m_liveDataCounter.Inc();
			evt.Actions.Add(NetworkEventType.MessageLiveData);
			var routes = sensor.LiveDataRouting.ToList(); // Take a snapshot

			foreach(var info in routes) {
				this.EnqueueTo(message, info);
			}

			return true;
		}

		private void EnqueueTo(IPlatformMessage message, RoutingTarget target)
		{
			switch(message.Type) {
			case MessageType.ControlMessage:
				this.m_logger.LogDebug("Queueing control message to {target}", target.Target);
				this.m_internalQueue.EnqueueControlMessageToTarget(message, target);
				break;
			case MessageType.Message:
				this.m_logger.LogDebug("Queueing message to {target}", target.Target);
				this.m_internalQueue.EnqueueMessageToTarget(message, target);
				break;

			case MessageType.Measurement:
				this.m_logger.LogDebug("Queueing measurement to {target}", target.Target);
				this.m_internalQueue.EnqueueMeasurementToTarget(message, target);
				break;
			}
		}
	}
}
