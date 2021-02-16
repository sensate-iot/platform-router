/*
 * Distributed cache options.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace SensateIoT.Platform.Network.Common.Caching.Abstract
{
	public sealed class DistributedCacheOptions : IOptions<DistributedCacheOptions>
	{
		public ConfigurationOptions Configuration { get; set; }
		public bool Gzip { get; set; }

		DistributedCacheOptions IOptions<DistributedCacheOptions>.Value => this;
	}
}
