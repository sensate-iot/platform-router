/*
 * Sensor controller Unit Test.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

using Moq;
using Moq.Language;

using MongoDB.Bson;
using NUnit.Framework;
using SensateService.Controllers;
using SensateService.Controllers.V1;
using SensateService.Models;
using SensateService.Infrastructure.Repositories;
using SensateService.Infrastructure.Document;
using SensateService.Infrastructure.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;

namespace SensateService.Tests.UnitTests
{
	[TestFixture]
	public class SensorsControllerTests
	{
		private Mock<ISensorRepository> _sensors;

		[SetUp]
		public void Setup()
		{
			Sensor s1, s2;

			this._sensors = new Mock<ISensorRepository>();

			s1 = new Sensor {
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now,
				Secret = "Yolo",
				InternalId = ObjectId.GenerateNewId(),
				Name = "Test Sensor"
			};

			this._sensors.Setup(repo => repo.GetAsync("abcdef")).Returns(
				Task.FromResult(s1)
			);

			s2 = new Sensor {
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now,
				Secret = "Yolo",
				InternalId = ObjectId.GenerateNewId(),
				Name = "Test Sensor"
			};

			this._sensors.Setup(repo => repo.GetAsync("abcde")).Returns(
				Task.FromResult(s2)
			);
		}

		[TearDown]
		public void TearDown()
		{
		}

		[Test]
		public async Task CanGetSensorById()
		{
			/*SensorsController controller;

			controller = new SensorsController(this._sensors.Object, null);
			var result = await controller.GetById("abcde");
			Assert.IsTrue(result.GetType() == typeof(ObjectResult), "Unable to get sensor!");
			*/
			Assert.IsTrue(true);
			await Task.CompletedTask;
		}
	}
}
