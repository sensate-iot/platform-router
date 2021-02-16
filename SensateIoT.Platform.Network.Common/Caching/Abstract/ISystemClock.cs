/*
 * Clock interface to manage cache internals.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

namespace SensateIoT.Platform.Network.Common.Caching.Abstract
{
	public interface ISystemClock
	{
		DateTimeOffset GetUtcNow();
	}
}
