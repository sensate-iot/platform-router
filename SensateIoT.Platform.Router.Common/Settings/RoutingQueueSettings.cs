/*
 * Routing publish interval settings.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

namespace SensateIoT.Platform.Router.Common.Settings
{
	public class RoutingQueueSettings
	{
		public TimeSpan PublicInterval { get; set; }
		public TimeSpan InternalInterval { get; set; }
		public int? DequeueBatchSize { get; set; }
		public string ActuatorTopicFormat { get; set; }
	}
}
