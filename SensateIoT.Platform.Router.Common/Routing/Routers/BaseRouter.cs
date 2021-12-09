/*
 * Base message router. Should be executed first.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.Extensions.Logging;

using SensateIoT.Platform.Router.Common.Exceptions;
using SensateIoT.Platform.Router.Common.Routing.Abstract;
using SensateIoT.Platform.Router.Contracts.DTO;
using SensateIoT.Platform.Router.Data.Abstract;
using SensateIoT.Platform.Router.Data.DTO;

namespace SensateIoT.Platform.Router.Common.Routing.Routers
{
	public class BaseRouter : IRouter
	{
		private readonly ILogger<BaseRouter> m_logger;

		public BaseRouter(ILogger<BaseRouter> logger)
		{
			this.m_logger = logger;
		}

		public string Name => "Base Router";

		public bool Route(Sensor sensor, IPlatformMessage message, NetworkEvent networkEvent)
		{
			switch(message.Type) {
			case MessageType.Measurement:
				networkEvent.MessageType = NetworkMessageType.Measurement;
				break;

			case MessageType.Message:
				networkEvent.MessageType = NetworkMessageType.Message;
				break;

			case MessageType.ControlMessage:
				networkEvent.MessageType = NetworkMessageType.ControlMessage;
				break;

			default:
				this.m_logger.LogWarning("Unable to determine message type of type {type}. Integer value: {integerValue}.",
								  message.Type.ToString("G"), message.Type.ToString("D"));
				throw new RouterException(this.Name, $"invalid message type {message.Type}");
			}

			return true;
		}
	}
}
