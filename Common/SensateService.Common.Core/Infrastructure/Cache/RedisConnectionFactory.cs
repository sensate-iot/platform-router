/*
 * Redis connection factory. A standardized way
 * to connect to Redis.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using Microsoft.Extensions.Options;
using SensateService.Config;
using StackExchange.Redis;

namespace SensateService.Infrastructure.Cache
{
	public class RedisConnectionFactory : IRedisConnectionFactory
	{
		private readonly Lazy<ConnectionMultiplexer> _connection;
		private readonly IOptions<RedisConfig> _config;

		public RedisConnectionFactory(IOptions<RedisConfig> config)
		{
			this._config = config;
			this._connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(this._config.Value.Host));
		}

		public ConnectionMultiplexer Connection()
		{
			return this._connection.Value;
		}
	}
}