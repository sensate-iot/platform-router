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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace SensateIoT.API.DashboardApi.Application
{
	public static class Program
	{
		private static string GetAppSettings()
		{
			return Environment.GetEnvironmentVariable("SENSATE_DASHBOARDAPI_APPSETTINGS") ?? "appsettings.json";
		}

		public static void Main(string[] args)
		{
			Console.WriteLine($"Starting Dashboard API {Assembly.GetExecutingAssembly().GetName().Version}");
			var builder = CreateHostBuilder(args);
			var host = builder.Build();
			host.Run();
		}

		private static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.UseContentRoot(Directory.GetCurrentDirectory())
				.ConfigureAppConfiguration((ctx, config) => {
					config.AddJsonFile(GetAppSettings(), optional: false, reloadOnChange: true);
					config.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
					config.AddEnvironmentVariables();
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
