/*
 * Sensate SQL database context.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class SensateSqlContext : IdentityDbContext
	{
		public new DbSet<SensateUser> Users { get; set; }
		public DbSet<UserSensor> UserSensors { get; set; }

		public SensateSqlContext(DbContextOptions<SensateSqlContext> options) :
			base(options)
		{}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<UserSensor>().HasKey(
				k => new { k.SensorId, k.UserId }
			);
		}
	}
}
