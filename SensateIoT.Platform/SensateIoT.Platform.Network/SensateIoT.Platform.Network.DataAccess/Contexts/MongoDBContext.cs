/*
 * MongoDB data context.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MongoDB.Driver;

using SensateIoT.Platform.Network.Data.Models;
using SensateIoT.Platform.Network.DataAccess.Config;

namespace SensateIoT.Platform.Network.DataAccess.Contexts
{
	public class MongoDBContext
	{
		private readonly IMongoClient m_client;
		private readonly MongoDBSettings m_settings;

		public IMongoDatabase Database => this.m_client.GetDatabase(this.m_settings.DatabaseName);
		public IMongoCollection<Sensor> Sensors => this.Database.GetCollection<Sensor>("Sensors");
		public IMongoCollection<Message> Messages => this.Database.GetCollection<Message>("Messages");
		public IMongoCollection<MeasurementBucket> Measurements => this.Database.GetCollection<MeasurementBucket>("Measurements");
		public IMongoCollection<SensorStatisticsEntry> SensorStatistics => this.Database.GetCollection<SensorStatisticsEntry>("Statistics");
		public IMongoCollection<ControlMessage> ControlMessages => this.Database.GetCollection<ControlMessage>("ControlMessages");

		public MongoDBContext(IOptions<MongoDBSettings> settings, ILogger<MongoDBContext> logger)
		{
			this.m_settings = settings.Value;

			try {
				var mongosettings = MongoClientSettings.FromUrl(new MongoUrl(settings.Value.ConnectionString));

				mongosettings.MaxConnectionPoolSize = settings.Value.MaxConnections;
				this.m_client = new MongoClient(mongosettings);
			} catch(Exception ex) {
				logger.LogWarning(
					"Unable to connect to MongoDB: {message}. Trace: {trace}. Exception: {exception}.",
					ex.Message,
					ex.StackTrace,
					ex);
			}
		}
	}
}
