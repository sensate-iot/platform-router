/*
 * Default system clock implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Runtime.CompilerServices;
using SensateIoT.Common.Caching.Abstract;

[assembly: InternalsVisibleTo("SensateIoT.Common.Caching.Tests")]
namespace SensateIoT.Common.Caching.Memory
{
	internal class SystemClock : ISystemClock
	{
		public DateTimeOffset GetUtcNow()
		{
			return DateTimeOffset.UtcNow;
		}
	}
}
