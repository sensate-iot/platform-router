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
		[Required]
		public string KeyValue { get; set; }
		public decimal? LowerEdge { get; set; }
		public decimal? UpperEdge { get; set; }
		[Required]
		public string SensorId { get; set; }
		[Required]
		public string Message { get; set; }
	}
}