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
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using SensateService.Converters;

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
				Description = "Unit testing sensor",
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

			BsonSerializer.RegisterSerializationProvider(new BsonDecimalSerializationProvider());
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

			m = await this._repo.GetMeasurementAsync(null, x => x.CreatedBy == this._sensor.InternalId);
			var list = m.Data as List<DataPoint>;

			Assert.True(list[0].Name == "z");
			Assert.True(list[1].Name == "x");
			Assert.True(list[2].Name == "y");
		}

		[Test, Order(1)]
		public async Task CanCreateMeasurement()
		{
			dynamic obj, measurement;
			JArray array;

			var x = typeof(decimal);
			var y = typeof(decimal?);

			measurement = new JObject();
			array = new JArray();

			obj = new JObject();
			obj.Value = 22.3949988317M;
			obj.Name = "z";
			array.Add(obj);

			obj = new JObject();
			obj.Value = 3.143611234211M;
			obj.Name = "x";
			array.Add(obj);

			obj = new JObject();
			obj.Value = 9.8136986919M;
			obj.Name = "y";
			array.Add(obj);

			measurement.Longitude = 1.1234;
			measurement.Latitude = 22.1165;
			measurement.Data = array;
			measurement.CreatedBySecret = "TestingSecret";

			await this._repo.ReceiveMeasurement(_sensor, measurement.ToString());
			Assert.IsTrue(this._receivedMeasurement != null);
		}
	}
}
