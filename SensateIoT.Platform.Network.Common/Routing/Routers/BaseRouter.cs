/*
 * Base message router. Should be executed first.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateIoT.Platform.Network.Common.Exceptions;
using SensateIoT.Platform.Network.Common.Routing.Abstract;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Common.Routing.Routers
{
	public class BaseRouter : IRouter
	{
		public bool Route(Sensor sensor, IPlatformMessage message, NetworkEvent networkEvent)
		{
			networkEvent.MessageType = message.Type switch {
				MessageType.Measurement => NetworkMessageType.Measurement,
				MessageType.Message => NetworkMessageType.Message,
				MessageType.ControlMessage => NetworkMessageType.ControlMessage,
				_ => throw new RouterException(nameof(BaseRouter), "unable to determine message type!")
			};

			return true;
		}
	}
}
