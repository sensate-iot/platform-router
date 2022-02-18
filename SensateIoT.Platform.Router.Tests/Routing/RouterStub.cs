using System;

using SensateIoT.Platform.Router.Common.Routing.Abstract;
using SensateIoT.Platform.Router.Contracts.DTO;
using SensateIoT.Platform.Router.Data.Abstract;
using SensateIoT.Platform.Router.Data.DTO;

namespace SensateIoT.Platform.Router.Tests.Routing
{
	public class RouterStub : IRouter
	{
		public bool Executed { get; private set; }
		public bool Cancel { get; set; }
		public Exception Exception { get; set; }

		public string Name => "Control Message Router";

		public RouterStub()
		{
			this.Executed = false;
			this.Cancel = false;
			this.Exception = null;
		}

		public bool Route(Sensor sensor, IPlatformMessage message, NetworkEvent networkEvent)
		{
			if(this.Cancel) {
				return false;
			}

			if(this.Exception != null) {
				throw this.Exception;
			}

			this.Executed = true;
			return true;
		}
	}
}
