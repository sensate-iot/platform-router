/*
 * Redis connection factory. A standardized way
 * to connect to Redis.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using StackExchange.Redis;

namespace SensateService.Infrastructure.Cache
{
	public interface IRedisConnectionFactory
	{
		ConnectionMultiplexer Connection();
	}
}