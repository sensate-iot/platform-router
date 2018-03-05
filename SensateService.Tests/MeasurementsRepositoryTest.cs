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
using SensateService.Models.Database;
using SensateService.Models.Database.Document;
using SensateService.Models.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SensateService.Tests
{
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

		[SetUp]
		public void SetUp()
		{
			MongoDBSettings settings;
			ILogger<AbstractMeasurementRepository> logger;
			SensateContext ctx;

			this._receivedMeasurement = null;
			settings = new MongoDBSettings();
			settings.DatabaseName = "SensateUnitTests";
			settings.ConnectionString = "mongodb://SensateUnitTests:Sensate@localhost:27017/SensateUnitTests";
			logger = new LoggerFactory().CreateLogger<AbstractMeasurementRepository>();
			ctx = new SensateContext(settings);
			var repo = new StandardMeasurementRepository(ctx, logger);
			this._repo = repo;
			repo.MeasurementReceived += OnMeasurementReceived_Handler;
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

			m = await this._repo.GetMeasurementAsync(null, x => x.CreatedBy == _sensor.InternalId);
			Assert.IsFalse(m == null, "Unable to perform ID lookup!");
		}

		[Test, Order(1)]
		public async Task CanCreateMeasurement()
		{
			dynamic obj;

			obj = new JObject();
			obj.Data = 12.3456D;
			obj.Longitude = 1.1234;
			obj.Latitude = 22.1165;
			obj.CreatedBySecret = "TestingSecret";

			await this._repo.ReceiveMeasurement(_sensor, obj.ToString());
			Assert.IsTrue(this._receivedMeasurement != null);
		}
	}
}
