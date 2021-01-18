/*
 * .NET core entry point.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.IO;
using System.Reflection;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SensateIoT.API.Common.ApiCore.Init;
using SensateIoT.API.Common.Core.Infrastructure.Sql;
using SensateIoT.API.Common.IdentityData.Models;

namespace SensateIoT.API.AuthApi.Application
{
	public class Program
	{
		private static void CreateUserRoles(IHost wh)
		{
			ILogger<Program> logger;
			SensateSqlContext ctx;

			using var scope = wh.Services.CreateScope();
			var services = scope.ServiceProvider;
			logger = services.GetRequiredService<ILogger<Program>>();

			try {
				logger.LogInformation("Creating user roles..");
				ctx = services.GetRequiredService<SensateSqlContext>();
				var roles = services.GetRequiredService<RoleManager<SensateRole>>();
				var manager = services.GetRequiredService<UserManager<SensateUser>>();

				var countTask = roles.Roles.CountAsync();
				countTask.Wait();

				if(countTask.Result > 0) {
					return;
				}

				var tsk = UserRoleSeed.Initialize(ctx, roles, manager);
				tsk.Wait();
			} catch(Exception ex) {
				logger.LogError($"Unable to create user roles: {ex.Message}");
			}
		}

		public static void Main(string[] args)
		{
			Console.WriteLine($"Starting Auth API {Assembly.GetExecutingAssembly().GetName().Version}");
			var builder = CreateHostBuilder(args);
			var host = builder.Build();
			CreateUserRoles(host);
			host.Run();
		}

		private static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.UseContentRoot(Directory.GetCurrentDirectory())
				.ConfigureAppConfiguration((ctx, config) => {
					config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
					config.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
					config.AddEnvironmentVariables();

					if(ctx.HostingEnvironment.IsDevelopment()) {
						config.AddUserSecrets<Program>();
					}
				})
				.ConfigureLogging((ctx, builder) => {
					builder.ClearProviders();
					builder.AddConsole();

					if(ctx.HostingEnvironment.IsDevelopment() || ctx.HostingEnvironment.IsStaging()) {
						builder.AddDebug();
					}
				})
				.ConfigureWebHostDefaults(webBuilder => {
					webBuilder.UseStartup<Startup>();
				});
		}

	}
}
