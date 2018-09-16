/*
 * Factory class for the Sensate SQL context.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.IO;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using SensateService.Config;
using SensateService.Infrastructure.Sql;

namespace SensateService.Setup
{
	public class SensateSqlContextFactory : IDesignTimeDbContextFactory<SensateSqlContext>
	{
		private IConfiguration Configuration;

		public SensateSqlContext CreateDbContext(string[] args)
		{
			var builder = new DbContextOptionsBuilder<SensateSqlContext>();
			var db = new DatabaseConfig(); 

			this.BuildConfiguration();
			this.Configuration.GetSection("Database").Bind(db);

			builder.UseNpgsql(db.PgSQL.ConnectionString, x => x.MigrationsAssembly("Setup"));
			return new SensateSqlContext(builder.Options);
		}

		private void BuildConfiguration()
		{
			var builder = new ConfigurationBuilder();

			builder.SetBasePath(Path.Combine(AppContext.BaseDirectory));
			builder.AddEnvironmentVariables();

			if(Program.IsDevelopment())
				builder.AddUserSecrets<Program>();
			else
                builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


			Configuration = builder.Build();
		}
	}
}
