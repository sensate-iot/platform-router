/*
 * Helper methods to create new database connections.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SensateIoT.Platform.Network.DataAccess.Config;
using SensateIoT.Platform.Network.DataAccess.Contexts;

namespace SensateIoT.Platform.Network.Common.Init
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

		public static void AddAuthorizationContext(this IServiceCollection services, string conn)
		{
			services.AddDbContextPool<AuthorizationContext>(options => { options.UseNpgsql(conn); }, 128);
		}

		public static void AddNetworkingContext(this IServiceCollection services, string conn)
		{
			services.AddDbContextPool<NetworkContext>(options => { options.UseNpgsql(conn); }, 128);
		}
	}
}
