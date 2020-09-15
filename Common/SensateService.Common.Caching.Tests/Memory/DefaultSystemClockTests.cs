﻿/*
 * Verify the default system clock implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using Xunit;

using SensateService.Common.Caching.Memory;

namespace SensateService.Common.Caching.Tests.Memory
{
	public class DefaultSystemClockTests
	{
		[Fact]
		public void DefaultSystemClockReturnsUtc()
		{
			var clock = new SystemClock();
			var now = DateTimeOffset.UtcNow;
			var clockNow = clock.GetUtcNow();

			Assert.Equal(now.Date, clockNow.Date);
			Assert.Equal(now.Hour, clockNow.Hour);
			Assert.Equal(now.Minute, clockNow.Minute);
			Assert.Equal(now.Second, clockNow.Second);
		}
	}
}