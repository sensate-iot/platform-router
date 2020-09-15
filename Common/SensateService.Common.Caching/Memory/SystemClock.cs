/*
 * Default system clock implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Runtime.CompilerServices;
using SensateService.Common.Caching.Abstract;

[assembly: InternalsVisibleTo("SensateService.Common.Caching.Tests")]
namespace SensateService.Common.Caching.Memory
{
	internal class SystemClock : ISystemClock
	{
		public DateTimeOffset GetUtcNow()
		{
			return DateTimeOffset.UtcNow;
		}
	}
}
