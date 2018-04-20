/*
 * Sensor controller Unit Test.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Threading.Tasks;

using Moq;
using MongoDB.Bson;
using NUnit.Framework;

using SensateService.Models;
using SensateService.Infrastructure.Repositories;

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
				Name = "Test Sensor",
				Description = "Sensor for unit testing purposes",
				Owner = "50692771-e625-4f7b-9eab-7d4e9dad1886"
			};

			this._sensors.Setup(repo => repo.GetAsync("abcdef")).Returns(
				Task.FromResult(s1)
			);

			s2 = new Sensor {
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now,
				Secret = "Yolo",
				InternalId = ObjectId.GenerateNewId(),
				Name = "Test Sensor",
				Description = "Sensor for unit testing purposes",
				Owner = "50692771-e625-4f7b-9eab-7d4e9dad1886"
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
			Assert.IsTrue(true);
			await Task.CompletedTask;
		}
	}
}
