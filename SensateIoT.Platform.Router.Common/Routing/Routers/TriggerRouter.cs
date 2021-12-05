﻿/*
 * Trigger service router.
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
	public class TriggerRouter : IRouter
	{
		private readonly IInternalRemoteQueue m_internalRemote;
		private readonly ILogger<TriggerRouter> m_logger;
		private readonly Counter m_counter;

		public string Name => "Trigger Router";

		public TriggerRouter(IInternalRemoteQueue queue, ILogger<TriggerRouter> logger)
		{
			this.m_internalRemote = queue;
			this.m_logger = logger;
			this.m_counter = Metrics.CreateCounter("router_trigger_messages_routed_total",
														   "Total number of routed trigger messages.");
		}

		public bool Route(Sensor sensor, IPlatformMessage message, NetworkEvent networkEvent)
		{
			if(message.Type == MessageType.ControlMessage) {
				return true;
			}

			if(sensor.TriggerInformation == null || sensor.TriggerInformation.Count <= 0) {
				return true;
			}

			this.ProcessMessage(sensor, message, networkEvent);
			return true;
		}

		private void ProcessMessage(Sensor sensor, IPlatformMessage message, NetworkEvent evt)
		{
			this.m_counter.Inc();
			var textTriggered = false;
			var measurementTriggered = false;
			var triggers = sensor.TriggerInformation.ToList(); // Snap shot

			foreach(var info in triggers) {
				var done = this.MatchTrigger(message, evt, info, ref textTriggered, ref measurementTriggered);

				if(done) {
					break;
				}
			}
		}

		private bool MatchTrigger(IPlatformMessage message, NetworkEvent evt, SensorTrigger info, ref bool textTriggered, ref bool measurementTriggered)
		{
			if(!VerifySensorTrigger(message, info)) {
				return false;
			}

			evt.Actions.Add(NetworkEventType.MessageTriggered);

			if(!textTriggered && info.IsTextTrigger) {
				textTriggered = true;
				this.EnqueueToTriggerService(message);
			} else if(!measurementTriggered && !info.IsTextTrigger) {
				measurementTriggered = true;
				this.EnqueueToTriggerService(message);
			}

			return textTriggered && measurementTriggered;
		}

		private static bool VerifySensorTrigger(IPlatformMessage message, SensorTrigger info)
		{
			if(!info.HasActions) {
				return false;
			}

			return (!info.IsTextTrigger || message.Type == MessageType.Message) && (info.IsTextTrigger || message.Type == MessageType.Measurement);
		}

		private void EnqueueToTriggerService(IPlatformMessage message)
		{
			switch(message.Type) {
			case MessageType.Measurement:
				this.m_internalRemote.EnqueueMeasurementToTriggerService(message);
				break;

			case MessageType.Message:
				this.m_internalRemote.EnqueueMessageToTriggerService(message);
				break;

			default:
				this.m_logger.LogError("Received invalid message type. Unable to route message to trigger service. " +
									   "The received type is: {type}", message.Type);
				throw new RouterException(nameof(TriggerRouter), $"invalid message type: {message.Type:G}");
			}
		}

	}
}