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

namespace SensateService.Infrastructure.Sql
{
	public sealed class SensateSqlContext : IdentityDbContext
	{
		public SensateSqlContext(DbContextOptions<SensateSqlContext> options) :
			base(options)
		{}
	}
}
