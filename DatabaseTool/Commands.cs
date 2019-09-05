/*
 * Collection of command implementations.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.DatabaseTool
{
	public delegate Task CliCommand(IServiceScope scope, string argument);

	public class Commands
	{
		public static async Task GetMeasurementsBySensor(IServiceScope scope, string args)
		{
			Stopwatch sw;

			if(args.Length <= 0) {
				Console.WriteLine("> usage: GetMeasurementsBySensor <sensorID>");
				return;
			}

			var repo = scope.ServiceProvider.GetRequiredService<IMeasurementRepository>();
			var sensors = scope.ServiceProvider.GetRequiredService<ISensorRepository>();

			try {
				var sensor = await sensors.GetAsync(args).AwaitBackground();
				sw = new Stopwatch();

				if(sensor == null)
					Console.WriteLine("> Unknown sensor ID");

				sw.Start();
				var measurements = await repo.GetMeasurementsBySensorAsync(sensor).AwaitBackground();
				sw.Stop();

				var enumerable = measurements as List<Measurement> ?? measurements.ToList();
				var json = JsonConvert.SerializeObject(enumerable.TakeLast(100), Formatting.Indented);

				Console.WriteLine(json);
				Console.WriteLine($"> Number of measurements stored: {enumerable.Count}");
				Console.WriteLine($"> Needed {sw.Elapsed} to complete query!");
			} catch(IndexOutOfRangeException) {
				Console.WriteLine("> Unknown sensor ID");
			}
		}

		public static async Task GetMeasurementsAfterYesterday(IServiceScope scope, string args)
		{
			Stopwatch sw;

			if(args.Length <= 0) {
				Console.WriteLine("> usage: GetMeasurementsBySensor <sensorID>");
				return;
			}

			var repo = scope.ServiceProvider.GetRequiredService<IMeasurementRepository>();
			var sensors = scope.ServiceProvider.GetRequiredService<ISensorRepository>();

			try {
				var sensor = await sensors.GetAsync(args).AwaitBackground();
				sw = new Stopwatch();

				if(sensor == null)
					Console.WriteLine("> Unknown sensor ID");

				var today = DateTime.Today;
				var yday = today.AddDays(-1D);

				sw.Start();
				var measurements = await repo.GetAfterAsync(sensor, yday).AwaitBackground();
				sw.Stop();

				var enumerable = measurements as List<Measurement> ?? measurements.ToList();
				var json = JsonConvert.SerializeObject(enumerable.TakeLast(100), Formatting.Indented);

				Console.WriteLine(json);
				Console.WriteLine($"> Number of measurements stored: {enumerable.Count}");
				Console.WriteLine($"> Needed {sw.Elapsed} to complete query!");
			} catch(IndexOutOfRangeException) {
				Console.WriteLine("> Unknown sensor ID");
			}
		}

		public static async Task GetMeasurementsBeforeToday(IServiceScope scope, string args)
		{
			Stopwatch sw;

			if(args.Length <= 0) {
				Console.WriteLine("> usage: GetMeasurementsBySensor <sensorID>");
				return;
			}

			var repo = scope.ServiceProvider.GetRequiredService<IMeasurementRepository>();
			var sensors = scope.ServiceProvider.GetRequiredService<ISensorRepository>();

			try {
				var sensor = await sensors.GetAsync(args).AwaitBackground();
				sw = new Stopwatch();

				if(sensor == null)
					Console.WriteLine("> Unknown sensor ID");

				var today = DateTime.Today;

				sw.Start();
				var measurements = await repo.GetBeforeAsync(sensor, today).AwaitBackground();
				sw.Stop();

				var enumerable = measurements as List<Measurement> ?? measurements.ToList();
				var json = JsonConvert.SerializeObject(enumerable.TakeLast(100), Formatting.Indented);

				Console.WriteLine(json);
				Console.WriteLine($"> Number of measurements stored: {enumerable.Count}");
				Console.WriteLine($"> Needed {sw.Elapsed} to complete query!");
			} catch(IndexOutOfRangeException) {
				Console.WriteLine("> Unknown sensor ID");
			}
		}
	}
}