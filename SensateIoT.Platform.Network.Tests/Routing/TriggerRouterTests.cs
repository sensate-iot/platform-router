/*
 * Trigger router unit tests.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using Moq;

using SensateIoT.Platform.Network.Common.Collections.Abstract;
using SensateIoT.Platform.Network.Common.Routing.Routers;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.Tests.Utility;
using Measurement = SensateIoT.Platform.Network.Data.DTO.Measurement;

namespace SensateIoT.Platform.Network.Tests.Routing
{
	[TestClass]
	public class TriggerRouterTests
	{
		[TestMethod]
		public void CanRouteDecimalTriggers()
		{
			var measurementCount = 0;
			var messageCount = 0;
			var evt = new NetworkEvent();
			var router = CreateTriggerRouter(() => measurementCount += 1, () => messageCount += 1);
			var sensor = new Sensor {
				ID = ObjectId.GenerateNewId()
			};

			var measurement = new Measurement {
				SensorId = sensor.ID
			};

			sensor.TriggerInformation ??= new List<SensorTrigger>();
			sensor.TriggerInformation.Add(new SensorTrigger {
				HasActions = true,
				IsTextTrigger = false
			});

			router.Route(sensor, measurement, evt);

			Assert.AreEqual(1, measurementCount);
			Assert.AreEqual(0, messageCount);
			Assert.AreEqual(1, evt.Actions.Count);
			Assert.AreEqual(NetworkEventType.MessageTriggered, evt.Actions[0]);
		}

		[TestMethod]
		public void CanRouteTextTriggers()
		{
			var measurementCount = 0;
			var messageCount = 0;
			var evt = new NetworkEvent();
			var router = CreateTriggerRouter(() => measurementCount += 1, () => messageCount += 1);
			var sensor = new Sensor {
				ID = ObjectId.GenerateNewId()
			};

			var message = new Message {
				SensorId = sensor.ID
			};

			sensor.TriggerInformation ??= new List<SensorTrigger>();
			sensor.TriggerInformation.Add(new SensorTrigger {
				HasActions = true,
				IsTextTrigger = true
			});

			router.Route(sensor, message, evt);

			Assert.AreEqual(0, measurementCount);
			Assert.AreEqual(1, messageCount);
			Assert.AreEqual(1, evt.Actions.Count);
			Assert.AreEqual(NetworkEventType.MessageTriggered, evt.Actions[0]);
		}

		[TestMethod]
		public void CannotRouteEmptyTriggers()
		{
			var measurementCount = 0;
			var messageCount = 0;
			var evt = new NetworkEvent();
			var router = CreateTriggerRouter(() => measurementCount += 1, () => messageCount += 1);
			var sensor = new Sensor {
				ID = ObjectId.GenerateNewId()
			};

			var message = new Message {
				SensorId = sensor.ID
			};

			sensor.TriggerInformation ??= new List<SensorTrigger>();
			sensor.TriggerInformation.Add(new SensorTrigger {
				HasActions = false,
				IsTextTrigger = true
			});

			router.Route(sensor, message, evt);

			Assert.AreEqual(0, measurementCount);
			Assert.AreEqual(0, messageCount);
			Assert.AreEqual(0, evt.Actions.Count);
		}

		[TestMethod]
		public void CanRouteEfficiently()
		{
			var measurementCount = 0;
			var messageCount = 0;
			var evt = new NetworkEvent();
			var router = CreateTriggerRouter(() => measurementCount += 1, () => messageCount += 1);
			var sensor = new Sensor {
				ID = ObjectId.GenerateNewId()
			};

			var message = new Message {
				SensorId = sensor.ID
			};

			sensor.TriggerInformation ??= new List<SensorTrigger>();
			sensor.TriggerInformation.Add(new SensorTrigger {
				HasActions = true,
				IsTextTrigger = false
			});

			router.Route(sensor, message, evt);

			Assert.AreEqual(0, measurementCount);
			Assert.AreEqual(0, messageCount);
			Assert.AreEqual(0, evt.Actions.Count);
		}

		private static TriggerRouter CreateTriggerRouter(Action measurementCallback, Action messageCallback)
		{
			var queue = new Mock<IInternalRemoteQueue>();
			var logger = new Mock<ILogger<TriggerRouter>>();

			queue.Setup(x => x.EnqueueMeasurementToTriggerService(It.IsAny<IPlatformMessage>()))
				.Callback(measurementCallback);
			queue.Setup(x => x.EnqueueMessageToTriggerService(It.IsAny<IPlatformMessage>()))
				.Callback(messageCallback);

			return new TriggerRouter(queue.Object, logger.Object);
		}
	}
}
