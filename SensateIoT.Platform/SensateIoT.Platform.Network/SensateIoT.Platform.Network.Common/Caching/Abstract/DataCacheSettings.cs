/*
 * Settings for the data cache.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

namespace SensateIoT.Platform.Network.Common.Caching.Abstract
{
	public class DataCacheSettings
	{
		public long? Capacity { get; set; }
		public TimeSpan Timeout { get; set; }
	}
}
