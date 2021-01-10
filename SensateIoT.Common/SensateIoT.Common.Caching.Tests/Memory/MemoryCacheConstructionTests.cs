/*
 * Tests for the (default) construction of a memory cache.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateIoT.Common.Caching.Memory;
using Xunit;

namespace SensateIoT.Common.Caching.Tests.Memory
{
	public class MemoryCacheConstructionTests : MemoryCache<int, int>
	{
		[Fact]
		public void DefaultConstructorCreatesAValidCache()
		{
			var cache = new MemoryCache<int, int>();

			Assert.Equal(0, cache.Count);
			Assert.Equal(-1L, cache.Capacity);
			Assert.Equal(0L, cache.Size);
		}

		[Fact]
		public void CanCreateMemoryCappedCache()
		{
			var cache = new MemoryCache<int, int>(100);

			Assert.Equal(100, cache.Capacity);
		}
	}
}