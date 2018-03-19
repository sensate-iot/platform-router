/*
 * Test the measurement storage model.
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
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NUnit.Framework;
using SensateService.Controllers;
using SensateService.Models;
using SensateService.Infrastructure.Repositories;
using SensateService.Infrastructure.Document;
using SensateService.Infrastructure.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SensateService.Tests
{
	public class TestMeasurement
	{
		public decimal y { get; set; }
		public string  x { get; set; }
	}

	[TestFixture]
	public class MeasurementsRepositoryTests
	{
		private IMeasurementRepository _repo;
		private Measurement _receivedMeasurement;
		private Sensor _sensor;

		public MeasurementsRepositoryTests()
		{
			_sensor = new Sensor {
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now,
				Unit = "V",
				Secret = "TestingSecret",
				InternalId = ObjectId.GenerateNewId(),
				Name = "Test Sensor"
			};
		}

		[OneTimeSetUp]
		public void SetUpOnce()
		{
			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
		}

		[OneTimeTearDown]
		public void TearDownOnce()
		{
			Trace.Flush();
		}

		[SetUp]
		public void SetUp()
		{
			MongoDBSettings settings;
			ILogger<MeasurementRepository> logger;
			SensateContext ctx;

			this._receivedMeasurement = null;
			settings = new MongoDBSettings();
			settings.DatabaseName = "SensateUnitTests";
			settings.ConnectionString = "mongodb://SensateUnitTests:Sensate@localhost:27017/SensateUnitTests";
			logger = new LoggerFactory().CreateLogger<MeasurementRepository>();
			ctx = new SensateContext(settings);
			var repo = new MeasurementRepository(ctx, logger);
			this._repo = repo;
			MeasurementEvents.MeasurementReceived += OnMeasurementReceived_Handler;
		}

		[TearDown]
		public void TearDown()
		{
		}

		private Task OnMeasurementReceived_Handler(object s, MeasurementReceivedEventArgs e)
		{
			this._receivedMeasurement = e.Measurement;
			return Task.CompletedTask;
		}

		[Test, Order(2)]
		public async Task CanGetById()
		{
			Measurement m;
			TestMeasurement testMeasurement;

			m = await this._repo.GetMeasurementAsync(null, x => x.CreatedBy == this._sensor.InternalId);
			testMeasurement = m.ConvertData<TestMeasurement>();

			Assert.IsFalse(testMeasurement == null, "Unable to create TestMeasurement object!");
			Assert.IsTrue(testMeasurement.x == "FooBar!", "Invalid Testmeasurement!");
			Assert.IsTrue(testMeasurement.y == 12351.1234567890M, "Invalid Testmeasurement!");
			Assert.IsFalse(m == null, "Unable to perform ID lookup!");
		}

		[Test, Order(1)]
		public async Task CanCreateMeasurement()
		{
			dynamic obj;
			TestMeasurement m = new TestMeasurement {
				y = 12351.1234567890M,
				x = "FooBar!"
			};

			obj = new JObject();
			obj.Data = JToken.Parse(m.ToJson());
			obj.Longitude = 1.1234;
			obj.Latitude = 22.1165;
			obj.CreatedBySecret = "TestingSecret";

			await this._repo.ReceiveMeasurement(_sensor, obj.ToString());
			Assert.IsTrue(this._receivedMeasurement != null);
		}
	}
}
