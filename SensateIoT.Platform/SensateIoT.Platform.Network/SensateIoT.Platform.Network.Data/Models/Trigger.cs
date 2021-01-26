/*
 * Trigger data model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Data.Models
{
	public class Trigger
	{
		public long ID { get; set; }
		public string SensorID { get; set; }
		public string KeyValue { get; set; }
		public double? LowerEdge { get; set; }
		public double? UpperEdge { get; set; }
		public string FormalLanguage { get; set; }
		public TriggerType Type { get; set; }

		public virtual ICollection<TriggerAction> TriggerActions { get; set; }
	}
}
