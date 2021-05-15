/*
 * Trigger service router.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

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

			this.MatchTrigger(sensor, message, networkEvent);
			return true;
		}

		private void MatchTrigger(Sensor sensor, IPlatformMessage message, NetworkEvent evt)
		{
			var textTriggered = false;
			var measurementTriggered = false;

			foreach(var info in sensor.TriggerInformation) {
				if(info.HasActions) {
					evt.Actions.Add(NetworkEventType.MessageTriggered);

					if(!textTriggered && info.IsTextTrigger) {
						textTriggered = true;
						this.EnqueueToTriggerService(message, info.IsTextTrigger);
					} else if(!measurementTriggered && !info.IsTextTrigger) {
						measurementTriggered = true;
						this.EnqueueToTriggerService(message, info.IsTextTrigger);
					}
				}

				if(textTriggered && measurementTriggered) {
					break;
				}
			}
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
