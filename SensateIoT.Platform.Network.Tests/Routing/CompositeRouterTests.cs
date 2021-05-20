using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MongoDB.Bson;
using Moq;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Collections.Abstract;
using SensateIoT.Platform.Network.Common.Routing;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Tests.Routing
{
	[TestClass]
	public class CompositeRouterTests
	{
		private static ObjectId SensorId = ObjectId.GenerateNewId();

		[TestMethod]
		public void CanExecuteRouters()
		{
			var router = CreateCompositeRouter();

			var r1 = new RouterStub();
			var r2 = new RouterStub();
			var r3 = new RouterStub();

			router.AddRouter(r1);
			router.AddRouter(r2);
			router.AddRouter(r3);

			var msg = new Message {
				SensorId = SensorId
			};
			router.Route(msg);

			Assert.IsTrue(r1.Executed);
			Assert.IsTrue(r2.Executed);
			Assert.IsTrue(r3.Executed);
		}

		[TestMethod]
		public void CanCancelRoutesGracefully()
		{
		}

		[TestMethod]
		public void CanCatchRouterExceptions()
		{
		}

		[TestMethod]
		public void CannotCatchOtherExceptions()
		{
		}

		[TestMethod]
		public void EnqueuesNetworkEvents()
		{
		}

		private static IRoutingCache CreateRoutingCache()
		{
			var cache = new Mock<IRoutingCache>();
			var sensor = new Sensor { ID = SensorId };

			cache.Setup(x => x[SensorId]).Returns(sensor);
			return cache.Object;
		}

		private static CompositeRouter CreateCompositeRouter()
		{
			var logger = new Mock<ILogger<CompositeRouter>>();
			var queue = new Mock<IRemoteNetworkEventQueue>();

			return new CompositeRouter(CreateRoutingCache(), queue.Object, logger.Object);
		}
	}
}
