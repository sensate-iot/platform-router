/*
 * Test the measurement storage model.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using NUnit.Framework;
using Newtonsoft.Json.Linq;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using SensateService.Converters;
using SensateService.Models;
using SensateService.Infrastructure.Repositories;
using SensateService.Infrastructure.Document;
using SensateService.Infrastructure.Events;
using SensateService.Models.Json.In;

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
			settings.ConnectionString = "mongodb://localhost:27017/SensateUnitTests";
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

			m = await this._repo.GetMeasurementAsync(x => x.CreatedBy == this._sensor.InternalId);
			var list = m.Data as List<DataPoint>;

			Assert.True(list[0].Name == "z");
			Assert.True(list[1].Name == "x");
			Assert.True(list[2].Name == "y");
			Assert.True(list[0].Value == 22.3949988317M);
		}

		[Test, Order(1)]
		public async Task CanCreateMeasurement()
		{
			dynamic obj;
			JArray array;
			RawMeasurement m;

			var x = typeof(decimal);
			var y = typeof(decimal?);

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

			m = new RawMeasurement {
				CreatedBySecret = "TestingSecret",
				CreatedById = _sensor.InternalId.ToString(),
				Longitude = 1.1234,
				Latitude = 22.123511,
				Data = array
			};

			await this._repo.ReceiveMeasurementAsync(_sensor, m);
			Assert.IsTrue(this._receivedMeasurement != null);
		}
	}
}
