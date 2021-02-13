using System;
using System.Linq;

using Microsoft.Extensions.Logging;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Routing;

namespace SensateIoT.Platform.Network.LoadTest.Routing
{
	public class RoutingCacheTests
	{
		private readonly IRoutingCache m_cache;

		public RoutingCacheTests()
		{
			var factory = LoggerFactory.Create(conf => {
				conf.AddConsole();
				conf.SetMinimumLevel(LogLevel.Information);
			});

			this.m_cache = new RoutingCache(factory.CreateLogger<RoutingCache>());
		}

		public void Populate(int count, int accountCount)
		{
			var rng = new Random();
			var accounts = SensorGenerator.GenerateAccounts(accountCount);

			Console.WriteLine("Generating cache entries...");
			SensorGenerator.PopulateCache(accounts.ToList(), rng, count, this.m_cache);
			Console.WriteLine("Finished generating cache entries.");
		}
	}
}