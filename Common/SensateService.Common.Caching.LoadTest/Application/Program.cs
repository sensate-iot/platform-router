/*
 * Load test entry point.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using SensateService.Common.Caching.LoadTest.MemoryTests;

namespace SensateService.Common.Caching.LoadTest.Application
{
    public class Program
    {
	    private const int ReadTestSize = 1000000;

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting load tests for the caching subsystem...");

            var reads = new ReadTests();
			reads.TestSynchronousRead(ReadTestSize);
			Console.WriteLine();
			Console.WriteLine("------------------------");
			Console.WriteLine();
			reads.TestAsynchronousRead(ReadTestSize).Wait();

#if !DEBUG
			Console.ReadLine();
#endif
        }
    }
}
