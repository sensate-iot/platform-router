/*
 * Composite router unit tests.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MongoDB.Bson;
using Moq;

using SensateIoT.Platform.Router.Common.Caching.Abstract;
using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Collections.Local;
using SensateIoT.Platform.Router.Common.Exceptions;
using SensateIoT.Platform.Router.Common.Routing;
using SensateIoT.Platform.Router.Common.Routing.Abstract;
using SensateIoT.Platform.Router.Common.Settings;
using SensateIoT.Platform.Router.Contracts.DTO;
using SensateIoT.Platform.Router.Data.Abstract;
using SensateIoT.Platform.Router.Data.DTO;

namespace SensateIoT.Platform.Router.Tests.Routing
{
	[TestClass]
	public class CompositeRouterTests
	{
		private static readonly Sensor Sensor = new() { ID = ObjectId.GenerateNewId(), AccountID = Guid.NewGuid(), SensorKey = "Abcd" };
		private static readonly Account Account = new() { ID = Sensor.AccountID };
		private static readonly ApiKey ApiKey = new() { AccountID = Sensor.AccountID };

		[TestMethod]
		public void CanExecuteRouters()
		{
			var msg = new Message {
				SensorId = Sensor.ID
			};

			var router = CreateCompositeRouter(Sensor, Account, ApiKey, CreateQueueWithMessage(msg));

			var r1 = new RouterStub();
			var r2 = new RouterStub();
			var r3 = new RouterStub();

			router.AddRouter(r1);
			router.AddRouter(r2);
			router.AddRouter(r3);

			var result = router.TryRoute(CancellationToken.None);

			Assert.IsTrue(r1.Executed);
			Assert.IsTrue(r2.Executed);
			Assert.IsTrue(r3.Executed);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void AccountGuidMustBeValid()
		{
			var account = new Account { ID = Guid.NewGuid(), HasBillingLockout = true };
			var apikey = new ApiKey { AccountID = account.ID };
			var sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = Guid.Empty };
			var queue = new MessageQueue(10);
			var router = CreateCompositeRouter(sensor, account, apikey, queue);

			TryExecuteRouter(router, sensor, queue);
		}

		[TestMethod]
		public void SensorKeyCannotBeNull()
		{
			var account = new Account { ID = Guid.NewGuid(), HasBillingLockout = true };
			var sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = account.ID };
			var queue = new MessageQueue(10);
			var router = CreateCompositeRouter(sensor, account, null, queue);

			TryExecuteRouter(router, sensor, queue);
		}

		[TestMethod]
		public void SensorCannotBeNull()
		{
			var msg = new Message {
				SensorId = ObjectId.GenerateNewId()
			};
			var router = CreateCompositeRouter(Sensor, Account, ApiKey, CreateQueueWithMessage(msg));

			var r1 = new RouterStub();
			var r2 = new RouterStub();
			var r3 = new RouterStub();

			router.AddRouter(r1);
			router.AddRouter(r2);
			router.AddRouter(r3);
			var result = router.TryRoute(CancellationToken.None);

			Assert.IsFalse(r1.Executed);
			Assert.IsFalse(r2.Executed);
			Assert.IsFalse(r3.Executed);
			Assert.IsFalse(result);
		}

		private static void TryExecuteRouter(IMessageRouter router, Sensor sensor, IQueue<IPlatformMessage> queue)
		{
			var r1 = new RouterStub();
			var r2 = new RouterStub();
			var r3 = new RouterStub();

			router.AddRouter(r1);
			router.AddRouter(r2);
			router.AddRouter(r3);

			var msg = new Message {
				SensorId = sensor.ID
			};
			queue.Add(msg);

			var result = router.TryRoute();

			Assert.IsFalse(r1.Executed);
			Assert.IsFalse(r2.Executed);
			Assert.IsFalse(r3.Executed);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void CanCancelRoutesGracefully()
		{
			var msg = new Message {
				SensorId = Sensor.ID
			};
			var router = CreateCompositeRouter(Sensor, Account, ApiKey, CreateQueueWithMessage(msg));

			var r1 = new RouterStub();
			var r2 = new RouterStub { Cancel = true };
			var r3 = new RouterStub();

			router.AddRouter(r1);
			router.AddRouter(r2);
			router.AddRouter(r3);

			var result = router.TryRoute(CancellationToken.None);

			Assert.IsTrue(r1.Executed);
			Assert.IsFalse(r2.Executed);
			Assert.IsFalse(r3.Executed);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void CanCatchRouterExceptions()
		{
			var msg = new Message {
				SensorId = Sensor.ID
			};
			var router = CreateCompositeRouter(Sensor, Account, ApiKey, CreateQueueWithMessage(msg));

			var r1 = new RouterStub();
			var r2 = new RouterStub { Exception = new RouterException("TestRouter", "testing exception catching.") };
			var r3 = new RouterStub();

			router.AddRouter(r1);
			router.AddRouter(r2);
			router.AddRouter(r3);
			var result = router.TryRoute(CancellationToken.None);

			Assert.IsTrue(r1.Executed);
			Assert.IsFalse(r2.Executed);
			Assert.IsFalse(r3.Executed);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void CannotCatchOtherExceptions()
		{
			var msg = new Message {
				SensorId = Sensor.ID
			};
			var router = CreateCompositeRouter(Sensor, Account, ApiKey, CreateQueueWithMessage(msg));

			var r1 = new RouterStub();
			var r2 = new RouterStub { Exception = new InvalidOperationException() };
			var r3 = new RouterStub();

			router.AddRouter(r1);
			router.AddRouter(r2);
			router.AddRouter(r3);
			Assert.ThrowsException<InvalidOperationException>(() => router.TryRoute(CancellationToken.None));
		}

		[TestMethod]
		public void EnqueuesNetworkEvents()
		{
			var count = 0;
			var options = Options.Create(new RoutingQueueSettings());
			var logger = new Mock<ILogger<CompositeRouter>>();
			var queue = new Mock<IRemoteNetworkEventQueue>();
			var msg = new Message {
				SensorId = Sensor.ID
			};

			queue.Setup(x => x.EnqueueEvent(It.IsAny<NetworkEvent>())).Callback(() => count += 1);
			var router = new CompositeRouter(CreateRoutingCache(Sensor, Account, ApiKey), CreateQueueWithMessage(msg), queue.Object, options, logger.Object);

			var r1 = new RouterStub();
			var r2 = new RouterStub();

			router.AddRouter(r1);
			router.AddRouter(r2);
			var result = router.TryRoute(CancellationToken.None);

			Assert.AreEqual(1, count);
			Assert.IsFalse(result);
		}

		private static IRoutingCache CreateRoutingCache(Sensor sensor, Account account, ApiKey key)
		{
			var cache = new Mock<IRoutingCache>();

			cache.Setup(x => x[sensor.ID]).Returns(sensor);
			cache.Setup(x => x.GetAccount(account.ID)).Returns(Account);
			cache.Setup(x => x.GetApiKey(sensor.SensorKey)).Returns(key);
			return cache.Object;
		}

		private static CompositeRouter CreateCompositeRouter(Sensor sensor, Account account, ApiKey key, IQueue<IPlatformMessage> inputQueue)
		{
			var logger = new Mock<ILogger<CompositeRouter>>();
			var queue = new Mock<IRemoteNetworkEventQueue>();
			var options = Options.Create(new RoutingQueueSettings());

			return new CompositeRouter(CreateRoutingCache(sensor, account, key), inputQueue, queue.Object, options, logger.Object);
		}

		private static IQueue<IPlatformMessage> CreateQueueWithMessage(IPlatformMessage message)
		{
			var queue = new MessageQueue(10);

			queue.Add(message);
			return queue;
		}
	}
}
