/*
 * Storage service program entry.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Reflection;
using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SensateIoT.Platform.Network.StorageService.Application
{
	public class Program
	{
		public static IHost CreateHost(string[] args)
		{
			Startup starter = null;

			var wh = Host.CreateDefaultBuilder(args)
				.UseContentRoot(Directory.GetCurrentDirectory())
				.ConfigureAppConfiguration((hostingContext, config) => {
					config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
					config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
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
			Console.WriteLine($"Starting StorageClient {Assembly.GetExecutingAssembly().GetName().Version}");

			var program = new ApplicationHost(CreateHost(args));
			program.Run();
		}
	}
}