/*
 * Extension method to initialise database repositories.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using SensateIoT.API.Common.Config.Config;
using SensateIoT.API.Common.Core.Converters;
using SensateIoT.API.Common.Core.Infrastructure;
using SensateIoT.API.Common.Core.Infrastructure.Cache;
using SensateIoT.API.Common.Core.Infrastructure.Document;
using SensateIoT.API.Common.Core.Infrastructure.Repositories;
using SensateIoT.API.Common.Core.Infrastructure.Sql;
using SensateIoT.API.Common.Core.Services.DataProcessing;
using SensateIoT.API.Common.Data.Models;
using SensateIoT.Common.Caching.Abstract;
using SensateIoT.Common.Caching.Memory;
using SensateIoT.Common.Caching.Redis;
using StackExchange.Redis;

namespace SensateIoT.API.Common.Core.Init
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
			services.AddScoped<ISensorLinkRepository, SensorLinkRepository>();

			return services;
		}

		public static IServiceCollection AddSensorServices(this IServiceCollection services)
		{
			services.AddScoped<ISensorService, SensorService>();
			return services;
		}

		public static IServiceCollection AddUserService(this IServiceCollection services)
		{
			services.AddScoped<IUserService, UserService>();
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

			services.AddSingleton<IMemoryCache<string, string>, MemoryCache<string, string>>();

			if(config.Type == "Distributed") {


				services.AddSingleton<IDistributedCache<string>>(p => {
					var options = new DistributedCacheOptions {
						Configuration = new ConfigurationOptions {
							EndPoints = { { db.Redis.Host, 6379 } },
							ClientName = "sensate-iot"
						}
					};
					return new RedisCache<string>(options);
				});

				services.AddScoped<ICacheStrategy<string>, DistributedCacheStrategy>();
			} else {
				services.AddScoped<ICacheStrategy<string>, MemoryCacheStrategy>();
			}

			return services;
		}
	}
}
