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
using Microsoft.EntityFrameworkCore;

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
		public string AuthorId { get; set; }
		[ForeignKey("AuthorId")]
		public SensateUser Author { get; set; }
		public DateTime Timestamp { get; set; }

		protected virtual void OnModelCreating(ModelBuilder builder)
		{
			builder.HasSequence<long>("Id")
				.StartsAt(0)
				.IncrementsBy(1);

			builder.Entity<AuditLog>()
				.Property(e => e.Id)
				.HasDefaultValueSql("nextval('\"Id\"')");
		}
	}
}
