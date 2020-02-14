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
        [Required]
        public long TriggerId { get; set; }
        [Required]
        public DateTimeOffset Timestamp { get; set; }
	}
}
