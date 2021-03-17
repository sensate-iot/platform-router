/*
 * Trigger action data model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Data.Models
{
	public class TriggerAction
	{
		public long ID { get; set; }
		public long TriggerID { get; set; }
		public TriggerChannel Channel { get; set; }
		public string Target { get; set; }
		public string Message { get; set; }
	}
}
