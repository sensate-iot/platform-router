/*
 * .NET services start up.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.IO;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SensateService.Processing.DataAuthorizationApi.Application
{
	public class Program
	{
		public static string GetAppSettings()
		{
			return Environment.GetEnvironmentVariable("SENSATE_AUTHORIZATIONAPI_APPSETTINGS") ?? "appsettings.json";
		}

		public static void Main(string[] args)
		{
			IHost wh;

			Console.WriteLine($"Starting DataAuthorization API using {Version.VersionString}");
			wh = BuildWebHost(args);
			wh.Run();
		}

		private static IHost BuildWebHost(string[] args)
		{
			var conf = new ConfigurationBuilder()
						.SetBasePath(Directory.GetCurrentDirectory())
						.AddJsonFile("hosting.json")
						.Build();

			var wh = Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder => {
					webBuilder.UseConfiguration(conf)
						.UseContentRoot(Directory.GetCurrentDirectory())
						.ConfigureAppConfiguration((hostingContext, config) => {
							config.AddJsonFile(GetAppSettings(), optional: false, reloadOnChange: true);
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
				});

			return wh.Build();
		}

	}
}
