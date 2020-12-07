/*
 * Load test entry point.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SensateIoT.Platform.Network.LoadTest.CacheTests;
using SensateIoT.Platform.Network.LoadTest.Config;
using SensateIoT.Platform.Network.LoadTest.RouterTest;

namespace SensateIoT.Platform.Network.LoadTest.Application
{
	public class Program
	{
		private const int ReadTestSize = 5000000;
		private const int ScanTestSize = 10000000;

		private static void RunMemoryTests()
		{
			var reads = new ReadTests();

			reads.GenerateIds(ReadTestSize);
			reads.TestSynchronousRead(ReadTestSize);
			Console.WriteLine();
			Console.WriteLine("------------------------");
			Console.WriteLine();

			reads.TestAsynchronousRead(ReadTestSize);
		}

		private static void RunScanTests()
		{
			Console.WriteLine();
			Console.WriteLine("------------------------");
			Console.WriteLine();

			var scans = new ScanTests();
			scans.GenerateIds(ScanTestSize);
			scans.ScanForTimeouts(ScanTestSize);
		}

		private static async Task RunRouterTest()
		{
			Console.WriteLine("Starting router tests!");
			Console.WriteLine("");

			var textSettings = await File.ReadAllTextAsync("appsettings.json").ConfigureAwait(false);
			var settings = JsonConvert.DeserializeObject<AppSettings>(textSettings);
			var sensorData = await File.ReadAllTextAsync(settings.SensorIdPath).ConfigureAwait(false);
			var sensors = JsonConvert.DeserializeObject<IList<string>>(sensorData);
			var generator = new MeasurementGenerator(sensors);
			var client = new RouterClient(settings.RouterHostname, settings.RouterPort);

			await client.RunAsync(generator).ConfigureAwait(false);
		}

		public static void Main(string[] args)
		{
			if(args.Length >= 1 && args[0] == "router-test") {
				RunRouterTest().Wait();
				return;
			}

			Console.WriteLine("Starting load tests for the caching subsystem...");

			RunMemoryTests();
			RunScanTests();
#if !DEBUG
			Console.ReadLine();
#endif
		}
	}
}