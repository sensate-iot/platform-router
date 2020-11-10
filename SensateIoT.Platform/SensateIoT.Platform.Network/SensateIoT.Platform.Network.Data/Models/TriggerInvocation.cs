/*
 * Trigger invocation model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

namespace SensateIoT.Platform.Network.Data.Models
{
	public class TriggerInvocation
	{
		public long ID { get; set; }
		public long TriggerActionID { get; set; }
		public DateTime Timestamp { get; set; }

		public virtual TriggerAction TriggerAction { get; set; }
	}
}
