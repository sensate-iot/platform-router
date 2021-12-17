/*
 * Router service main.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.IO;
using System.Reflection;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SensateIoT.Platform.Router.Service.Init;

using Serilog;

namespace SensateIoT.Platform.Router.Service.Application
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			var config = BuildConfiguration(env);
			Log.Logger = LoggingUtility.BuildLogger(config, env);

			Log.Logger.Information("Starting {app} {version}.",
								   Assembly.GetExecutingAssembly().GetName().Name,
								   Assembly.GetExecutingAssembly().GetName().Version);
			var host = CreateHostBuilder(args, config).Build();
			host.Run();
		}

		private static IConfigurationRoot BuildConfiguration(string env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables();

			return builder.Build();
		}

		private static IHostBuilder CreateHostBuilder(string[] args, IConfiguration conf)
		{
			return Host.CreateDefaultBuilder(args)
				.UseContentRoot(Directory.GetCurrentDirectory())
				.ConfigureLogging((context, builder) => {
					builder.ClearProviders();
					builder.AddSerilog();

					if(context.HostingEnvironment.IsDevelopment() || context.HostingEnvironment.IsStaging()) {
						builder.AddDebug();
					}
				})
				.ConfigureAppConfiguration((_, config) => {
					config.AddConfiguration(conf);
				})
				.ConfigureWebHostDefaults(webBuilder => {
					webBuilder.UseStartup<Startup>().UseKestrel(opts => opts.ConfigureEndpoints());
				});
		}

	}
}
