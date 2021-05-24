/*
 * Helper to create composite routers.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using Moq;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Collections.Abstract;
using SensateIoT.Platform.Network.Common.Routing;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Tests.Utility
{
	public static class CompositeRouterHelper
	{
		public static readonly Sensor Sensor = new Sensor { ID = ObjectId.GenerateNewId() };

		public static IRoutingCache CreateRoutingCache()
		{
			var cache = new Mock<IRoutingCache>();

			cache.Setup(x => x[Sensor.ID]).Returns(Sensor);
			return cache.Object;
		}

		public static CompositeRouter CreateCompositeRouter()
		{
			var logger = new Mock<ILogger<CompositeRouter>>();
			var queue = new Mock<IRemoteNetworkEventQueue>();

			return new CompositeRouter(CreateRoutingCache(), queue.Object, logger.Object);
		}
	}
}