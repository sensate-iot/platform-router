/*
 * .NET core entry point.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Linq;
using System.IO;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

using SensateService.Services;
using SensateService.Infrastructure.Sql;
using SensateService.Init;
using SensateService.Models;
using System.Diagnostics;

namespace SensateService
{
	public class Program
	{
		public const string ApiVersionString = "v1";

		public static void CreateUserRoles(IWebHost wh)
		{
			ILogger<Program> logger;
			SensateSqlContext ctx;

			using(var scope = wh.Services.CreateScope()) {
				var services = scope.ServiceProvider;
				logger = services.GetRequiredService<ILogger<Program>>();

				try {
					logger.LogInformation("Creating user roles..");
					ctx = services.GetRequiredService<SensateSqlContext>();
					var roles = services.GetRequiredService<RoleManager<UserRole>>();
					var manager = services.GetRequiredService<UserManager<SensateUser>>();

					if(ctx.Roles.Any())
						return;

					var tsk = UserRoleSeed.Initialize(ctx, roles, manager);
					tsk.Wait();
				} catch(Exception ex) {
					logger.LogError($"Unable to create user roles: {ex.Message}");
				}
			}
		}

		public static void Main(string[] args)
		{
			IWebHost wh = Program.BuildWebHost(args);

			Program.CreateUserRoles(wh);
			wh.Run();
		}

		public static IWebHost BuildWebHost(string[] args)
		{
			var conf = new ConfigurationBuilder()
						.SetBasePath(Directory.GetCurrentDirectory())
						.AddJsonFile("hosting.json")
						.Build();

			var wh = WebHost.CreateDefaultBuilder(args)
				.UseConfiguration(conf)
				.UseKestrel()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.ConfigureAppConfiguration((hostingContext, config) => {
					var env = hostingContext.HostingEnvironment;
					config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						 .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
					config.AddEnvironmentVariables();
				})
				.ConfigureLogging((hostingContext, logging) => {
					logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					logging.AddConsole();
					logging.AddDebug();
				})
				.UseStartup<Startup>();
			return wh.Build();
		}
	}
}
