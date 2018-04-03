/*
 * Helper methods to create new database connections.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

using MongoDB.Bson.Serialization;

using SensateService.Converters;
using SensateService.Infrastructure.Document;
using SensateService.Infrastructure.Sql;

namespace SensateService
{
	public static class DatabaseInitExtensions
	{
		public static void AddDocumentStore(this IServiceCollection services, string conn, string db)
		{
			services.Configure<MongoDBSettings>(options => {
				options.DatabaseName = db;
				options.ConnectionString = conn;
			});

			BsonSerializer.RegisterSerializationProvider(new BsonDecimalSerializationProvider());
		}

		public static void AddPostgres(this IServiceCollection services, string conn)
		{
			services.AddEntityFrameworkNpgsql()
					.AddDbContext<SensateSqlContext>(options => {
				options.UseNpgsql(conn);
			});

			services.AddTransient<SensateContext>();
		}
	}
}
