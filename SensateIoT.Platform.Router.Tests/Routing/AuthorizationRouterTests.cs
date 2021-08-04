using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MongoDB.Bson;
using Moq;

using SensateIoT.Platform.Router.Common.Caching.Abstract;
using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Routing;
using SensateIoT.Platform.Router.Common.Routing.Abstract;
using SensateIoT.Platform.Router.Common.Routing.Routers;
using SensateIoT.Platform.Router.Data.Abstract;
using SensateIoT.Platform.Router.Data.DTO;

namespace SensateIoT.Platform.Router.Tests.Routing
{
	[TestClass]
	public class AuthorizationRouterTests
	{
		private static readonly Sensor Sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = Guid.NewGuid(), SensorKey = "Abcd" };
		private static readonly Account Account = new Account { ID = Sensor.AccountID };

		[TestMethod]
		public void CannotRouteFromBannedAccount()
		{
			var account = new Account { ID = Guid.NewGuid(), HasBillingLockout = false, IsBanned = true };
			var sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = account.ID, SensorKey = "ABC" };
			var apikey = new ApiKey { AccountID = account.ID };
			var router = CreateCompositeRouter(sensor, account, apikey);

			this.TryExecuteRouter(router, sensor, account, apikey);
		}

		[TestMethod]
		public void CannotRouteBillingLockedAccount()
		{
			var account = new Account { ID = Guid.NewGuid(), HasBillingLockout = true };
			var sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = account.ID, SensorKey = "ABC" };
			var apikey = new ApiKey { AccountID = account.ID };
			var router = CreateCompositeRouter(sensor, account, apikey);

			this.TryExecuteRouter(router, sensor, account, apikey);
		}

		[TestMethod]
		public void CannotRouteWithReadOnlyKey()
		{
			var account = new Account { ID = Guid.NewGuid() };
			var sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = account.ID, SensorKey = "ABC" };
			var apikey = new ApiKey { AccountID = account.ID, IsReadOnly = true };
			var router = CreateCompositeRouter(sensor, account, apikey);

			this.TryExecuteRouter(router, sensor, account, apikey);
		}

		[TestMethod]
		public void CannotRouteWithRevokedKey()
		{
			var account = new Account { ID = Guid.NewGuid() };
			var sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = account.ID, SensorKey = "ABC" };
			var apikey = new ApiKey { AccountID = account.ID, IsRevoked = true };
			var router = CreateCompositeRouter(sensor, account, apikey);

			this.TryExecuteRouter(router, sensor, account, apikey);
		}

		[TestMethod]
		public void CannotRouteWithoutAccount()
		{
			var account = new Account { ID = Guid.NewGuid(), HasBillingLockout = true };
			var sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = Guid.NewGuid(), SensorKey = "ABC" };
			var apikey = new ApiKey { AccountID = account.ID };
			var router = CreateCompositeRouter(sensor, account, apikey);

			this.TryExecuteRouter(router, sensor, account, apikey);
		}

		[TestMethod]
		public void CannotRouteWithoutSensorKey()
		{
			var account = new Account { ID = Guid.NewGuid(), HasBillingLockout = true };
			var sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = account.ID, SensorKey = "ABC" };
			var router = CreateCompositeRouter(sensor, account, null);

			this.TryExecuteRouter(router, sensor, account, null);
		}

		[TestMethod]
		public void CannotExecuteWithEmptySensorKey()
		{
			var account = new Account { ID = Guid.NewGuid(), HasBillingLockout = true };
			var sensor = new Sensor { ID = ObjectId.GenerateNewId(), AccountID = account.ID, SensorKey = "" };
			var router = CreateCompositeRouter(sensor, account, null);
			var key = new ApiKey {
				AccountID = account.ID,
				IsReadOnly = false,
				IsRevoked = false
			};

			this.TryExecuteRouter(router, sensor, account, key);
		}


		private void TryExecuteRouter(IMessageRouter router, Sensor sensor, Account account, ApiKey key)
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

			router.Route(AsList(msg));

			Assert.IsFalse(r1.Executed);
			Assert.IsFalse(r2.Executed);
			Assert.IsFalse(r3.Executed);
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

		private static CompositeRouter CreateCompositeRouter(Sensor sensor, Account account, ApiKey key)
		{
			var logger = new Mock<ILogger<CompositeRouter>>();
			var queue = new Mock<IRemoteNetworkEventQueue>();

			return new CompositeRouter(CreateRoutingCache(sensor, account, key), queue.Object, logger.Object);
		}

		private static AuthorizationRouter CreateAuthorizationRouter(Action measurementCallback, Action messageCallback, Sensor s, Account a, ApiKey key)
		{
			var queue = new Mock<IInternalRemoteQueue>();
			var logger = new Mock<ILogger<AuthorizationRouter>>();

			queue.Setup(x => x.EnqueueMeasurementToTriggerService(It.IsAny<IPlatformMessage>()))
				.Callback(measurementCallback);
			queue.Setup(x => x.EnqueueMessageToTriggerService(It.IsAny<IPlatformMessage>()))
				.Callback(messageCallback);

			return new AuthorizationRouter(CreateRoutingCache(s, a, key), logger.Object);
		}

	}
}