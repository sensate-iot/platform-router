/*
 * Default system clock implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Runtime.CompilerServices;

using SensateIoT.Platform.Network.Common.Caching.Abstract;

[assembly: InternalsVisibleTo("SensateIoT.Platform.Network.Tests")]
namespace SensateIoT.Platform.Network.Common.Caching.Memory
{
	internal class SystemClock : ISystemClock
	{
		public DateTimeOffset GetUtcNow()
		{
			return DateTimeOffset.UtcNow;
		}
	}
}
