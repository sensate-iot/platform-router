/*
 * Verify the default system clock implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SensateIoT.Platform.Network.Common.Caching.Memory;

namespace SensateIoT.Platform.Network.Tests.Caching.Memory
{
	[TestClass]
	public class DefaultSystemClockTests
	{
		[TestMethod]
		public void DefaultSystemClockReturnsUtc()
		{
			var clock = new SystemClock();
			var now = DateTimeOffset.UtcNow;
			var clockNow = clock.GetUtcNow();

			Assert.AreEqual(now.Date, clockNow.Date);
			Assert.AreEqual(now.Hour, clockNow.Hour);
			Assert.AreEqual(now.Minute, clockNow.Minute);
			Assert.AreEqual(now.Second, clockNow.Second);
		}
	}
}
