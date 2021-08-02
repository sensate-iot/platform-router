/*
 * Helper methods to create new database connections.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using Microsoft.Extensions.DependencyInjection;
using SensateIoT.Platform.Router.DataAccess.Abstract;
using SensateIoT.Platform.Router.DataAccess.Config;
using SensateIoT.Platform.Router.DataAccess.Contexts;
using SensateIoT.Platform.Router.DataAccess.Settings;

namespace SensateIoT.Platform.Router.Common.Init
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

			services.AddSingleton<MongoDBContext>();
		}

		public static void AddAuthorizationContext(this IServiceCollection services)
		{
			services.AddScoped<IAuthorizationDbContext, AuthorizationDbContext>();
		}

		public static void AddNetworkingContext(this IServiceCollection services)
		{
			services.AddScoped<INetworkingDbContext, NetworkingDbContext>();
		}

		public static void AddConnectionStrings(this IServiceCollection services, string networking, string authorization)
		{
			services.Configure<DatabaseSettings>(opts => {
				opts.NetworkingDbConnectionString = networking;
				opts.AuthorizationDbConnectionString = authorization;
			});
		}
	}
}
