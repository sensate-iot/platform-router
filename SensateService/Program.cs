/*
 * .NET core entry point.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System.IO;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;

using SensateService.Services;

namespace SensateService
{
	public class Program
	{
		public static MqttService MqttClient;

		public const string ApiVersionString = "v1";

		public static void Main(string[] args)
		{
			var wh = new WebHostBuilder()
				.UseKestrel()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.ConfigureAppConfiguration((hostingContext, config) => {
					var env = hostingContext.HostingEnvironment;
					config.AddJsonFile("appsettings.json", optional:false, reloadOnChange:true)
						 .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional:true, reloadOnChange:true);
					config.AddEnvironmentVariables();
				})
				.ConfigureLogging((hostingContext, logging) => {
					logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					logging.AddConsole();
					logging.AddDebug();
				})
				.UseStartup<Startup>();

			wh.Build().Run();
		}

		public static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.Build();
	}
}
