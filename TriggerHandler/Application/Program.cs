/*
 * TriggerHandler entry point.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SensateService.TriggerHandler.Application
{
	public class Program
	{
		public static string GetAppSettings()
		{
			return Environment.GetEnvironmentVariable("SENSATE_TRIGGERHANDLER_APPSETTINGS") ?? "appsettings.json";
		}

		public static IHost CreateHost(string[] args)
		{
			Startup starter = null;

			var wh = Host.CreateDefaultBuilder(args)
				.UseContentRoot(Directory.GetCurrentDirectory())
				.ConfigureAppConfiguration((hostingContext, config) => {
					if(hostingContext.HostingEnvironment.IsProduction())
						config.AddJsonFile(GetAppSettings(), optional: false, reloadOnChange: true);
					else
						config.AddUserSecrets<Startup>();
					config.AddEnvironmentVariables();
				})
				.ConfigureLogging((hostingContext, logging) => {
					logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					logging.AddConsole();

					if(hostingContext.HostingEnvironment.IsDevelopment())
						logging.AddDebug();
				})
				.ConfigureServices((ctx, services) => {
					starter = new Startup(ctx.Configuration);
					starter.ConfigureServices(services);
				});

			var host = wh.Build();
			var provider = host.Services;

			if(starter == null)
				return null;

			starter.Configure(provider);
			return host;
		}

		public static void Main(string[] args)
		{
			Console.WriteLine($"Starting TriggerHandler using {Version.VersionString}");

			var program = new Application(CreateHost(args));
			program.Run();
		}
	}
}
