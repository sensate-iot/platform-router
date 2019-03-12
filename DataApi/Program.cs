/*
 * .NET core entry point.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.IO;
using System.Linq;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SensateService.ApiCore.Init;
using SensateService.Infrastructure.Sql;
using SensateService.Models;

namespace SensateService.DataApi
{
	public class Program
	{
		private static void CreateUserRoles(IWebHost wh)
		{
			ILogger<Program> logger;
			SensateSqlContext ctx;

			using(var scope = wh.Services.CreateScope()) {
				var services = scope.ServiceProvider;
				logger = services.GetRequiredService<ILogger<Program>>();

				try {
					logger.LogInformation("Creating user roles..");
					ctx = services.GetRequiredService<SensateSqlContext>();
					var roles = services.GetRequiredService<RoleManager<SensateRole>>();
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
			IWebHost wh;

			Console.WriteLine($"Starting {Version.VersionString}");
			wh = BuildWebHost(args);
			CreateUserRoles(wh);
			wh.Run();
		}

		private static IWebHost BuildWebHost(string[] args)
		{
			var conf = new ConfigurationBuilder()
						.SetBasePath(Directory.GetCurrentDirectory())
						.AddJsonFile("hosting.json")
						.Build();

			var wh = WebHost.CreateDefaultBuilder(args)
				.UseConfiguration(conf)
				.UseContentRoot(Directory.GetCurrentDirectory())
				.ConfigureAppConfiguration((hostingContext, config) => {
					if(hostingContext.HostingEnvironment.IsProduction())
						config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
					else
						config.AddUserSecrets<Startup>();
					config.AddEnvironmentVariables();
				})
				.ConfigureLogging((hostingContext, logging) => {
					logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					logging.AddConsole();
					logging.AddDebug();
				})
				.UseStartup<Startup>()
				.ConfigureKestrel((ctx, opts) => {
					opts.AllowSynchronousIO = true;
				});

			return wh.Build();
		}
	}
}
