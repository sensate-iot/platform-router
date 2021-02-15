/*
 * Trigger API DTO.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.ComponentModel.DataAnnotations;
using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.API.DTO
{
	public class RawTrigger
	{
		public string KeyValue { get; set; }
		public double? LowerEdge { get; set; }
		public double? UpperEdge { get; set; }
		public string FormalLanguage { get; set; }
		[Required]
		public TriggerType Type { get; set; }
		[Required]
		public string SensorId { get; set; }
	}
}
