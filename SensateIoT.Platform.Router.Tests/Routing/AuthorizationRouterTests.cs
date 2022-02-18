using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MongoDB.Bson;
using Moq;

using SensateIoT.Platform.Router.Common.Caching.Abstract;
using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Collections.Local;
using SensateIoT.Platform.Router.Common.Routing;
using SensateIoT.Platform.Router.Common.Routing.Abstract;
using SensateIoT.Platform.Router.Common.Routing.Routers;
using SensateIoT.Platform.Router.Common.Settings;
using SensateIoT.Platform.Router.Data.Abstract;
using SensateIoT.Platform.Router.Data.DTO;

namespace SensateIoT.Platform.Router.Tests.Routing
{
	[TestClass]
	public class AuthorizationRouterTests
	{
		private static readonly Sensor Sensor = new() { ID = ObjectId.GenerateNewId(), AccountID = Guid.NewGuid(), SensorKey = "Abcd" };
		private static readonly Account Account = new() { ID = Sensor.AccountID };

		[TestMethod]
		public void CannotRouteFromBannedAccount()
		{
			var account = new Account { ID = Guid.NewGuid(), HasBillingLockout = false, IsBanned = true };
			var sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = account.ID, SensorKey = "ABC" };
			var apikey = new ApiKey { AccountID = account.ID };
			var messageQueue = new MessageQueue(10);
			var router = CreateCompositeRouter(sensor, account, apikey, messageQueue);

			this.TryExecuteRouter(router, sensor, account, apikey, messageQueue);
		}

		[TestMethod]
		public void CannotRouteBillingLockedAccount()
		{
			var account = new Account { ID = Guid.NewGuid(), HasBillingLockout = true };
			var sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = account.ID, SensorKey = "ABC" };
			var apikey = new ApiKey { AccountID = account.ID };
			var messageQueue = new MessageQueue(10);
			var router = CreateCompositeRouter(sensor, account, apikey, messageQueue);

			this.TryExecuteRouter(router, sensor, account, apikey, messageQueue);
		}

		[TestMethod]
		public void CannotRouteWithReadOnlyKey()
		{
			var account = new Account { ID = Guid.NewGuid() };
			var sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = account.ID, SensorKey = "ABC" };
			var apikey = new ApiKey { AccountID = account.ID, IsReadOnly = true };
			var messageQueue = new MessageQueue(10);
			var router = CreateCompositeRouter(sensor, account, apikey, messageQueue);

			this.TryExecuteRouter(router, sensor, account, apikey, messageQueue);
		}

		[TestMethod]
		public void CannotRouteWithRevokedKey()
		{
			var account = new Account { ID = Guid.NewGuid() };
			var sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = account.ID, SensorKey = "ABC" };
			var apikey = new ApiKey { AccountID = account.ID, IsRevoked = true };
			var messageQueue = new MessageQueue(10);
			var router = CreateCompositeRouter(sensor, account, apikey, messageQueue);

			this.TryExecuteRouter(router, sensor, account, apikey, messageQueue);
		}

		[TestMethod]
		public void CannotRouteWithoutAccount()
		{
			var account = new Account { ID = Guid.NewGuid(), HasBillingLockout = true };
			var sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = Guid.NewGuid(), SensorKey = "ABC" };
			var apikey = new ApiKey { AccountID = account.ID };
			var messageQueue = new MessageQueue(10);
			var router = CreateCompositeRouter(sensor, account, apikey, messageQueue);

			this.TryExecuteRouter(router, sensor, account, apikey, messageQueue);
		}

		[TestMethod]
		public void CannotRouteWithoutSensorKey()
		{
			var account = new Account { ID = Guid.NewGuid(), HasBillingLockout = true };
			var sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = account.ID, SensorKey = "ABC" };
			var messageQueue = new MessageQueue(10);
			var router = CreateCompositeRouter(sensor, account, null, messageQueue);

			this.TryExecuteRouter(router, sensor, account, null, messageQueue);
		}

		[TestMethod]
		public void CannotExecuteWithEmptySensorKey()
		{
			var account = new Account { ID = Guid.NewGuid(), HasBillingLockout = true };
			var sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = account.ID, SensorKey = "" };
			var messageQueue = new MessageQueue(10);
			var router = CreateCompositeRouter(sensor, account, null, messageQueue);
			var key = new ApiKey {
				AccountID = account.ID,
				IsReadOnly = false,
				IsRevoked = false
			};

			this.TryExecuteRouter(router, sensor, account, key, messageQueue);
		}


		private void TryExecuteRouter(IMessageRouter router, Sensor sensor, Account account, ApiKey key, IQueue<IPlatformMessage> queue)
		{
			var r1 = new RouterStub();
			var r2 = new RouterStub();
			var r3 = new RouterStub();
			var auth = CreateAuthorizationRouter(() => { }, () => { }, sensor, account, key);

			router.AddRouter(auth);
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

		private static IList<IPlatformMessage> AsList(IPlatformMessage message)
		{
			return new List<IPlatformMessage> { message };
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

		private static AuthorizationRouter CreateAuthorizationRouter(Action measurementCallback, Action messageCallback, Sensor s, Account a, ApiKey key)
		{
			var queue = new Mock<IRemoteTriggerQueue>();
			var logger = new Mock<ILogger<AuthorizationRouter>>();

			queue.Setup(x => x.EnqueueMeasurementToTriggerService(It.IsAny<IPlatformMessage>()))
				.Callback(measurementCallback);
			queue.Setup(x => x.EnqueueMessageToTriggerService(It.IsAny<IPlatformMessage>()))
				.Callback(messageCallback);

			return new AuthorizationRouter(CreateRoutingCache(s, a, key), logger.Object);
		}

	}
}
