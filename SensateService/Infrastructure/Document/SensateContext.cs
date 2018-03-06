/*
 * Sensate database context (MongoDB).
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
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

		public SensateContext(IOptions<MongoDBSettings> settings) :
			this(settings.Value)
		{
		}

		public SensateContext(MongoDBSettings settings)
		{
			MongoClient client;

			try {
				MongoClientSettings mongosettings = MongoClientSettings.FromUrl(new MongoUrl(
					settings.ConnectionString
				));
				client = new MongoClient(mongosettings);
				this._client = client;
				this._db = client.GetDatabase(settings.DatabaseName);
			} catch(Exception ex) {
				Console.WriteLine("Unable to connect to MongoDB!");
				throw ex;
			}

		}

		public IMongoCollection<T> Set<T>(string name)
		{
			return this._db.GetCollection<T>(name);
		}
	}
}
