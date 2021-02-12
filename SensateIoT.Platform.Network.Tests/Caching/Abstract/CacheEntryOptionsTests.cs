/*
 * Unit tests for the entry cache options.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SensateIoT.Platform.Network.Common.Caching.Abstract;

namespace SensateIoT.Platform.Network.Tests.Caching.Abstract
{
	[TestClass]
	public class CacheEntryOptionsTests
	{
		[TestMethod]
		public void CacheEntryOptionsDefaultToEmpty()
		{
			var opts = new CacheEntryOptions();

			Assert.IsNull(opts.Size);
			Assert.IsNull(opts.Timeout);
		}

		[TestMethod]
		public void CacheEntryTimeoutConvertsToMilliseconds()
		{
			var opts = new CacheEntryOptions {
				Timeout = TimeSpan.FromSeconds(5)
			};

			Assert.AreEqual(5000, opts.Timeout.Value.TotalMilliseconds);
		}
	}
}
