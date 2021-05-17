/*
 * Trigger service router.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Linq;

using Microsoft.Extensions.Logging;

using SensateIoT.Platform.Network.Common.Collections.Abstract;
using SensateIoT.Platform.Network.Common.Exceptions;
using SensateIoT.Platform.Network.Common.Routing.Abstract;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Common.Routing.Routers
{
	public class TriggerRouter : IRouter
	{
		private readonly IInternalRemoteQueue m_internalRemote;
		private readonly ILogger<TriggerRouter> m_logger;

		public TriggerRouter(IInternalRemoteQueue queue, ILogger<TriggerRouter> logger)
		{
			this.m_internalRemote = queue;
			this.m_logger = logger;
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
			if(!info.HasActions) {
				return textTriggered && measurementTriggered;
			}

			evt.Actions.Add(NetworkEventType.MessageTriggered);

			if(!textTriggered && info.IsTextTrigger) {
				textTriggered = true;
				this.EnqueueToTriggerService(message, info.IsTextTrigger);
			} else if(!measurementTriggered && !info.IsTextTrigger) {
				measurementTriggered = true;
				this.EnqueueToTriggerService(message, info.IsTextTrigger);
			}

			return textTriggered && measurementTriggered;
		}

		private void EnqueueToTriggerService(IPlatformMessage message, bool isText)
		{
			switch(message.Type) {
			case MessageType.Measurement when isText:
				return;

			case MessageType.Measurement:
				this.m_internalRemote.EnqueueMeasurementToTriggerService(message);
				break;

			case MessageType.Message:
				this.m_internalRemote.EnqueueToMessageTriggerService(message);
				break;

			default:
				this.m_logger.LogError("Received invalid message type. Unable to route message to trigger service. " +
									   "The received type is: {type}", message.Type);
				throw new RouterException(nameof(TriggerRouter), $"invalid message type: {message.Type:G}");
			}
		}


	}
}
