/*
 * DataCache reload settings.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using JetBrains.Annotations;

namespace SensateIoT.Platform.Network.Common.Settings
{
	[UsedImplicitly]
	public class DataReloadSettings
	{
		public bool EnableReload { get; set; }
		public TimeSpan DataReloadInterval { get; set; }
		public TimeSpan StartDelay { get; set; }
		public TimeSpan TimeoutScanInterval { get; set; }
		public TimeSpan LiveDataReloadInterval { get; set; }
	}
}
