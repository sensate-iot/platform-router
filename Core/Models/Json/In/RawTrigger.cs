/*
 * Raw trigger model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.ComponentModel.DataAnnotations;

namespace SensateService.Models.Json.In
{
	public class RawTrigger
	{
		public string KeyValue { get; set; }
		public decimal? LowerEdge { get; set; }
		public decimal? UpperEdge { get; set; }
		public string FormalLanguage { get; set; }
		[Required]
		public string SensorId { get; set; }
	}
}