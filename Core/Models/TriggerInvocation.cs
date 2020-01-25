/*
 * Trigger invocation model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace SensateService.Models
{
	public class TriggerInvocation
	{
        [Required]
        public long Id { get; set; }
		[StringLength(24, MinimumLength = 24)]
		public string MeasurementBucketId { get; set; }
        [Required]
        public int MeasurementId { get; set; }
        [Required]
        public long TriggerId { get; set; }
        [Required]
        public DateTimeOffset Timestamp { get; set; }
	}
}
