/*
 * Sensate database context (MongoDB).
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;

using Microsoft.Extensions.Options;
using MongoDB.Driver;

using SensateService.Models;

namespace SensateService.Infrastructure.Document
{
	public sealed class SensateContext
	{
		private readonly IMongoDatabase _db;
		private readonly IMongoClient _client;

		public IMongoCollection<MeasurementBucket> Measurements => this._db.GetCollection<MeasurementBucket>("Measurements");
		public IMongoCollection<Measurement> MeasurementData => this._db.GetCollection<Measurement>("Measurements");
		public IMongoCollection<Sensor> Sensors => this._db.GetCollection<Sensor>("Sensors");
		public IMongoCollection<AuditLog> Logs => this._db.GetCollection<AuditLog>("Logs");
		public IMongoCollection<SensorStatisticsEntry> Statistics => this._db.GetCollection<SensorStatisticsEntry>("Statistics");

		public SensateContext(IOptions<MongoDBSettings> options) : this(options.Value)
		{ }

		public SensateContext(MongoDBSettings settings)
		{
			try {
				MongoClientSettings mongosettings = MongoClientSettings.FromUrl(new MongoUrl(
					settings.ConnectionString
				));

				mongosettings.MaxConnectionPoolSize = settings.MaxConnections;
				this._client = new MongoClient(mongosettings);
				this._db = this._client.GetDatabase(settings.DatabaseName);
			} catch(Exception ex) {
				Console.WriteLine("Unable to connect to MongoDB!");
				throw ex;
			}

		}
	}
}
