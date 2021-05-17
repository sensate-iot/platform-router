using SensateIoT.Platform.Network.Common.Routing.Abstract;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Tests.Routing
{
	public class RouterStub : IRouter
	{
		public bool Executed { get; private set; }

		public bool Route(Sensor sensor, IPlatformMessage message, NetworkEvent networkEvent)
		{
			this.Executed = true;
			return true;
		}
	}
}