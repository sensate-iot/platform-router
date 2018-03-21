/*
 * Extension method to initialise database repositories.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SensateService.Infrastructure.Cache;
using SensateService.Infrastructure.Document;
using SensateService.Infrastructure.Repositories;
using SensateService.Infrastructure.Sql;

namespace SensateService.Init
{
	public static class RepositoryInitExtensions
	{
		public static IServiceCollection AddSqlRepositories(this IServiceCollection services)
		{
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IChangeEmailTokenRepository, ChangeEmailTokenRepository>();
			services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
			services.AddScoped<ISensateRoleRepository, SensateRoleRepository>();
			services.AddScoped<IAuditLogRepository, AuditLogRepository>();

			return services;
		}

		public static IServiceCollection AddMongoDbRepositories(
			this IServiceCollection services, bool cache
		)
		{
			if(cache) {
				Debug.WriteLine("Caching enabled!");
				services.AddScoped<IMeasurementRepository, CachedMeasurementRepository>();
				services.AddScoped<ISensorRepository, CachedSensorRepository>();
			} else {
				Debug.WriteLine("Caching disabled!");
				services.AddScoped<IMeasurementRepository, MeasurementRepository>();
				services.AddScoped<ISensorRepository, SensorRepository>();
			}
			return services;
		}

		public static IServiceCollection AddCacheStrategy(
			this IServiceCollection services, string type
		)
		{
			if(type == "Distributed") {
				services.AddScoped<ICacheStrategy<string>, DistributedCacheStrategy>();
			} else {
				services.AddScoped<ICacheStrategy<string>, MemoryCacheStrategy>();
			}

			return services;
		}
	}
}
