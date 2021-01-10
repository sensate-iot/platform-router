using System;
using Microsoft.EntityFrameworkCore;

namespace SensateIoT.API.SqlSetup.Application
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Running SensateService SQL setup...");
			var factory = new SensateSqlContextFactory();
			var ctx = factory.CreateDbContext(new string[] { });

			ctx.Database.EnsureCreated();
			ctx.Database.Migrate();
		}

		public static bool IsDevelopment()
		{
			var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
			return env == "Development";
		}
	}
}
