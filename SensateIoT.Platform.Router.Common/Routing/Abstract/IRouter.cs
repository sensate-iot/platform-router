/*
 * Composite routing interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateIoT.Platform.Router.Contracts.DTO;
using SensateIoT.Platform.Router.Data.Abstract;
using SensateIoT.Platform.Router.Data.DTO;

namespace SensateIoT.Platform.Router.Common.Routing.Abstract
{
	public interface IRouter
	{
		public string Name { get; }
		bool Route(Sensor sensor, IPlatformMessage message, NetworkEvent networkEvent);
	}
}
