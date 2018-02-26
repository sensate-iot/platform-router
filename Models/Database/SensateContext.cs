/*
 * Sensate database context (MongoDB).
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;

using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace SensateService.Models.Database
{
	public sealed class SensateContext
	{
		private readonly IMongoDatabase _db;

		public IMongoCollection<Measurement> Measurements
		{
			get {
				return this._db.GetCollection<Measurement>("Measurements");
			}
		}

		public IMongoCollection<Sensor> Sensors
		{
			get {
				return this._db.GetCollection<Sensor>("Sensors");
			}
		}

		public SensateContext(IOptions<MongoDBSettings> settings)
		{
			var client = new MongoClient(settings.Value.ConnectionString);

			if(client != null)
				this._db = client.GetDatabase(settings.Value.DatabaseName);
		}
	}
}
