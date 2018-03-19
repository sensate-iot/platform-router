/*
 * Sensor meta data model
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace SensateService.Models
{
	public class AuditLog
	{
		[Key]
		public long Id { get; set; }
		[Required]
		public string Route { get; set; }
		public SensateUser Author { get; set; }
		public DateTime Timestamp { get; set; }
	}
}
