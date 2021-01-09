using Microsoft.Extensions.DependencyInjection;

using SensateIoT.Platform.Ingress.DataAccess.Abstract;
using SensateIoT.Platform.Ingress.DataAccess.Config;
using SensateIoT.Platform.Ingress.DataAccess.Contexts;
using SensateIoT.Platform.Ingress.DataAccess.Repositories;

namespace SensateIoT.Platform.Ingress.DataAccess.Init
{
	public static class MongoDbInitExtensions
	{
		public static IServiceCollection AddMongoDb(this IServiceCollection collection, string conn, string db, int max)
		{
			collection.Configure<MongoDBSettings>(options => {
				options.DatabaseName = db;
				options.ConnectionString = conn;
				options.MaxConnections = max;
			});

			collection.AddSingleton<MongoDBContext>();
			collection.AddScoped<ISensorRepository, SensorRepository>();

			return collection;
		}
	}
}