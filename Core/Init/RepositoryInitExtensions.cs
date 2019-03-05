/*
 * Extension method to initialise database repositories.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson.Serialization;

using SensateService.Config;
using SensateService.Converters;
using SensateService.Infrastructure;
using SensateService.Infrastructure.Cache;
using SensateService.Infrastructure.Document;
using SensateService.Infrastructure.Repositories;
using SensateService.Infrastructure.Sql;
using SensateService.Models;

namespace SensateService.Init
{
	public static class RepositoryInitExtensions
	{
		public static IServiceCollection AddSqlRepositories(this IServiceCollection services, bool cache)
		{
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IChangeEmailTokenRepository, ChangeEmailTokenRepository>();
			services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

			if(cache)
				services.AddScoped<IUserRepository, CachedUserRepository>();
			else
				services.AddScoped<IUserRoleRepository, UserRoleRepository>();

			services.AddScoped<IAuditLogRepository, AuditLogRepository>();
			services.AddScoped<IUserTokenRepository, UserTokenRepository>();
			services.AddScoped<IChangePhoneNumberTokenRepository, ChangePhoneNumberRepository>();
			services.AddScoped<IBulkWriter<AuditLog>, AuditLogRepository>();

			return services;
		}

		public static IServiceCollection AddDocumentRepositories(this IServiceCollection services, bool cache)
		{
			BsonSerializer.RegisterSerializer(typeof(DateTime), new BsonUtcDateTimeSerializer());
			services.AddScoped<ISensorStatisticsRepository, SensorStatisticsRepository>();

			if(cache) {
				services.AddScoped<IMeasurementRepository, CachedMeasurementRepository>();
				services.AddScoped<ISensorRepository, SensorRepository>();
				services.AddScoped<IBulkWriter<Measurement>, CachedMeasurementRepository>();
			} else {
				services.AddScoped<IMeasurementRepository, MeasurementRepository>();
				services.AddScoped<ISensorRepository, SensorRepository>();
				services.AddScoped<IBulkWriter<Measurement>, MeasurementRepository>();
			}

			return services;
		}

		public static IServiceCollection AddCacheStrategy(this IServiceCollection services, CacheConfig config, DatabaseConfig db)
		{
			if(config.Type == "Distributed") {
                services.AddDistributedRedisCache(opts => {
					opts.Configuration = db.Redis.Host;
					opts.InstanceName = db.Redis.InstanceName;
				});

				services.AddScoped<ICacheStrategy<string>, DistributedCacheStrategy>();
			} else {
				services.AddMemoryCache();
				services.AddScoped<ICacheStrategy<string>, MemoryCacheStrategy>();
			}

			return services;
		}
	}
}
