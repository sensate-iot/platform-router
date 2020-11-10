/*
 * Trigger data model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Data.Models
{
	public class Trigger
	{
		public long ID { get; set; }
		public string SensorID { get; set; }
		public decimal? LowerEdge { get; set; }
		public decimal? UpperEdge { get; set; }
		public string FormalLanguage { get; set; }
		public TriggerType Type { get; set; }
	}
}
