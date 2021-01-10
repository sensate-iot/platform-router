/*
 * Clock interface to manage cache internals.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

namespace SensateIoT.Common.Caching.Abstract
{
	public interface ISystemClock
	{
		DateTimeOffset GetUtcNow();
	}
}
