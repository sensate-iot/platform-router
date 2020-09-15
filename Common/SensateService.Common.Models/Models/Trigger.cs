/*
 * Data trigger model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SensateService.Common.Data.Models
{
	public enum TriggerType
	{
		Number,
		Regex
	}

	public class Trigger
	{
		[Required, Key]
		public long Id { get; set; }
		[Required]
		public string KeyValue { get; set; }
		public decimal? LowerEdge { get; set; }
		public decimal? UpperEdge { get; set; }
		public string FormalLanguage { get; set; }
		[Required, StringLength(24, MinimumLength = 24)]
		public string SensorId { get; set; }
		[Required]
		public TriggerType Type { get; set; }
		public ICollection<TriggerAction> Actions { get; set; }
		public ICollection<TriggerInvocation> Invocations { get; set; }
	}
}
