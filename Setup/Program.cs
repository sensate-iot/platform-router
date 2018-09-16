
using Microsoft.Extensions.Configuration;
using SensateService.Config;
using System;
using System.Threading;

namespace SensateService.Setup
{
	public class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Running SensateService setup...");
		}

		public static bool IsDevelopment()
		{
			var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
			return env == "Development";
		}
	}
}
