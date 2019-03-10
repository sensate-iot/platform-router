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
		}

		[TearDown]
		public void TearDown()
		{
		}

	}
}
