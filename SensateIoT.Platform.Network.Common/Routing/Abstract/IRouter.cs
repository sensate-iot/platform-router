/*
 * Composite routing interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.Extensions.Logging;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Common.Routing.Abstract
{
	public interface IRouter
	{
		bool Route(Sensor sensor, IPlatformMessage message, NetworkEvent networkEvent, ILogger logger);
	}
}
