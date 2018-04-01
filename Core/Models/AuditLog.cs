/*
 * Sensor meta data model
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Net;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SensateService.Enums;

namespace SensateService.Models
{
	[Table("AspNetAuditLogs")]
	public class AuditLog
	{
		[Key]
		public long Id { get; set; }
		[Required]
		public string Route { get; set; }
		[Required]
		public RequestMethod Method { get; set; }
		[Required]
		public IPAddress Address { get; set; }
		public SensateUser Author { get; set; }
		public DateTime Timestamp { get; set; }
	}
}
