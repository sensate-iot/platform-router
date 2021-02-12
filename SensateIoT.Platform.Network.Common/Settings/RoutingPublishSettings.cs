/*
 * Routing publish interval settings.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

namespace SensateIoT.Platform.Network.Common.Settings
{
	public class RoutingPublishSettings
	{
		public TimeSpan PublicInterval { get; set; }
		public TimeSpan InternalInterval { get; set; }
		public string ActuatorTopicFormat { get; set; }
	}
}
