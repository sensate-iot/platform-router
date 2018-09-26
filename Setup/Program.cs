
using Microsoft.Extensions.Configuration;
using SensateService.Config;
using System;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace SensateService.Setup
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Running SensateService setup...");
			var factory = new SensateSqlContextFactory();
			var ctx = factory.CreateDbContext(new string[] {});

			ctx.Database.EnsureCreated();
			ctx.Database.Migrate();
		}

		public static bool IsDevelopment()
		{
			var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
			return env == "Development";
		}
	}
}
