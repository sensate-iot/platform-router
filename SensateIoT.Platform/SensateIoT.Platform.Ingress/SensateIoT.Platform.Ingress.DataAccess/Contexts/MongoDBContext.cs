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

using SensateIoT.Platform.Ingress.DataAccess.Config;
using SensateIoT.Platform.Ingress.DataAccess.Models;

namespace SensateIoT.Platform.Ingress.DataAccess.Contexts
{
	public class MongoDBContext
	{
		private readonly IMongoClient m_client;
		private readonly MongoDBSettings m_settings;

		public IMongoDatabase Database => this.m_client.GetDatabase(this.m_settings.DatabaseName);
		public IMongoCollection<Sensor> Sensors => this.Database.GetCollection<Sensor>("Sensors");

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
