/*
 * Tests for the (default) construction of a memory cache.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SensateIoT.Platform.Network.Common.Caching.Memory;

namespace SensateIoT.Platform.Network.Tests.Caching.Memory
{
	[TestClass]
	public class MemoryCacheConstructionTests
	{
		[TestMethod]
		public void DefaultConstructorCreatesAValidCache()
		{
			var cache = new MemoryCache<int, int>();

			Assert.AreEqual(0, cache.Count);
			Assert.AreEqual(-1L, cache.Capacity);
			Assert.AreEqual(0L, cache.Size);
		}

		[TestMethod]
		public void CanCreateMemoryCappedCache()
		{
			var cache = new MemoryCache<int, int>(100);

			Assert.AreEqual(100, cache.Capacity);
		}
	}
}
