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
		public long ActionID { get; set; }
		public long TriggerID { get; set; }
		public DateTime Timestamp { get; set; }
		public virtual TriggerAction Action { get; set; }
	}
}
