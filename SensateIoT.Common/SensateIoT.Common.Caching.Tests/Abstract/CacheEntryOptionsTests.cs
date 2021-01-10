/*
 * Unit tests for the entry cache options.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using SensateIoT.Common.Caching.Abstract;
using Xunit;

namespace SensateIoT.Common.Caching.Tests.Abstract
{
	public class CacheEntryOptionsTests
	{
		[Fact]
		public void CacheEntryOptionsDefaultToEmpty()
		{
			var opts = new CacheEntryOptions();

			Assert.Null(opts.Size);
			Assert.Null(opts.Timeout);
		}

		[Fact]
		public void CacheEntryTimeoutConvertsToMilliseconds()
		{
			var opts = new CacheEntryOptions {
				Timeout = TimeSpan.FromSeconds(5)
			};

			Assert.Equal(5000, opts.GetTimeoutMs());
		}
	}
}