/*
 * Helper methods to create new database connections.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using SensateIoT.API.Common.Core.Converters;
using SensateIoT.API.Common.Core.Infrastructure.Document;
using SensateIoT.API.Common.Core.Infrastructure.Sql;

namespace SensateIoT.API.Common.Core.Init
{
	public static class DatabaseInitExtensions
	{
		public static void AddDocumentStore(this IServiceCollection services, string conn, string db, int max)
		{
			services.Configure<MongoDBSettings>(options => {
				options.DatabaseName = db;
				options.ConnectionString = conn;
				options.MaxConnections = max;
			});

			BsonSerializer.RegisterSerializationProvider(new BsonDecimalSerializationProvider());
			services.AddSingleton<SensateContext>();
		}

		public static void AddPostgres(this IServiceCollection services, string sensateiot)
		{
			services.AddDbContextPool<SensateSqlContext>(options => {
				options.UseNpgsql(sensateiot);
			}, 256);
		}

		public static void AddPostgres(this IServiceCollection services, string sensateiot, string network)
		{
			services.AddDbContextPool<SensateSqlContext>(options => {
				options.UseNpgsql(sensateiot);
			}, 256);

			services.AddDbContextPool<NetworkContext>(options => {
				options.UseNpgsql(network);
			}, 256);
		}
	}
}
