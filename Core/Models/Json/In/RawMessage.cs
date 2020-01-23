/*
 * Message view model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace SensateService.Models.Json.In
{
	public class RawMessage
	{
        [StringLength(1024, MinimumLength = 1)]
        [Required]
        public string Data { get; set; }
        [Required]
		public string SensorId { get; set; }
        public DateTime? CreatedAt { get; set; }
	}
}

