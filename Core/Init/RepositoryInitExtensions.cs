/*
 * Extension method to initialise database repositories.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
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
using SensateService.Services;
using SensateService.Services.Processing;

namespace SensateService.Init
{
	public static class RepositoryInitExtensions
	{
		public static IServiceCollection AddSqlRepositories(this IServiceCollection services, bool cache)
		{
			services.AddScoped<IChangeEmailTokenRepository, ChangeEmailTokenRepository>();
			services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

			if(cache) {
				services.AddScoped<IUserRepository, CachedUserRepository>();
				services.AddScoped<IApiKeyRepository, CachedApiKeyRepository>();
			} else {
				services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
				services.AddScoped<IUserRepository, UserRepository>();
			}

			services.AddScoped<IUserRoleRepository, UserRoleRepository>();
			services.AddScoped<IUserTokenRepository, UserTokenRepository>();
			services.AddScoped<IChangePhoneNumberTokenRepository, ChangePhoneNumberRepository>();
			services.AddScoped<IBulkWriter<AuditLog>, AuditLogRepository>();
			services.AddScoped<IAuditLogRepository, AuditLogRepository>();
			services.AddScoped<ITriggerRepository, TriggerRepository>();
			services.AddScoped<IBlobRepository, BlobRepository>();

			return services;
		}

		public static IServiceCollection AddDocumentRepositories(this IServiceCollection services, bool cache)
		{
			BsonSerializer.RegisterSerializer(typeof(DateTime), new BsonUtcDateTimeSerializer());

			services.AddScoped<ISensorStatisticsRepository, SensorStatisticsRepository>();
			services.AddScoped<IMessageRepository, MessageRepository>();
			services.AddScoped<IControlMessageRepository, ControlMessageRepository>();
			services.AddScoped<IGeoQueryService, GeoQueryService>();

			if(cache) {
				services.AddScoped<IMeasurementRepository, CachedMeasurementRepository>();
				services.AddScoped<ISensorRepository, CachedSensorRepository>();
			} else {
				services.AddScoped<IMeasurementRepository, MeasurementRepository>();
				services.AddScoped<ISensorRepository, SensorRepository>();
			}

			return services;
		}

		public static IServiceCollection AddCacheStrategy(this IServiceCollection services, CacheConfig config, DatabaseConfig db)
		{
			services.AddMemoryCache();

			if(config.Type == "Distributed") {
                services.AddDistributedRedisCache(opts => {
					opts.Configuration = db.Redis.Host;
					opts.InstanceName = db.Redis.InstanceName;
				});

				services.AddScoped<ICacheStrategy<string>, DistributedCacheStrategy>();
			} else {
				services.AddScoped<ICacheStrategy<string>, MemoryCacheStrategy>();
			}

			return services;
		}
	}
}
