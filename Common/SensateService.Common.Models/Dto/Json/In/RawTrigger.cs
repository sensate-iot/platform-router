/*
 * Raw trigger model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.ComponentModel.DataAnnotations;
using SensateService.Common.Data.Models;

namespace SensateService.Common.Data.Dto.Json.In
{
	public class RawTrigger
	{
		public string KeyValue { get; set; }
		public decimal? LowerEdge { get; set; }
		public decimal? UpperEdge { get; set; }
		public string FormalLanguage { get; set; }
		[Required]
		public TriggerType Type { get; set; }
		[Required]
		public string SensorId { get; set; }
	}
}