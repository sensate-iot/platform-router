/*
 * Database tool entry point.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using Microsoft.Extensions.DependencyInjection;

namespace SensateService.DatabaseTool
{
	public class Program
	{
		public static Startup Application { get; private set; }

		public static void Main(string[] args)
		{
			IServiceCollection collection;
			Startup startup;

			startup = new Startup();
			collection = new ServiceCollection();

            Console.WriteLine($"Starting DatabaseTool using {Version.VersionString}");
			startup.ConfigureServices(collection);
			var provider = collection.BuildServiceProvider();
			startup.Configure(provider);
			Application = startup;
			startup.Run(provider).Wait();
		}
	}
}
