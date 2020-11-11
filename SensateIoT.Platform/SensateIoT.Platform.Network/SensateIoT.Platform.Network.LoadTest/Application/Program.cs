/*
 * Load test entry point.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using SensateIoT.Platform.Network.LoadTest.CacheTests;

namespace SensateIoT.Platform.Network.LoadTest.Application
{
	public class Program
	{
		private const int ReadTestSize = 1000000;

		private static void RunMemoryTests()
		{
			var reads = new ReadTests();
			reads.TestSynchronousRead(ReadTestSize);
			Console.WriteLine();
			Console.WriteLine("------------------------");
			Console.WriteLine();

			reads.TestAsynchronousRead(ReadTestSize).Wait();
		}

		private static void RunDistributedTests()
		{
			//var tests = new GetSetTests();
			//tests.Run().Wait();
		}

		public static void Main(string[] args)
		{
			Console.WriteLine("Starting load tests for the caching subsystem...");

			RunMemoryTests();
			RunDistributedTests();
#if !DEBUG
			Console.ReadLine();
#endif
		}
	}
}