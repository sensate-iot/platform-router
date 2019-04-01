/*
 * Measurement statistics repository.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

using SensateService.ApiCore.Controllers;
using SensateService.DataApi.Json;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.Out;

namespace SensateService.DataApi.Controllers
{
	[Produces("application/json")]
	[Route("[controller]")]
	public class StatisticsController : AbstractApiController 
	{
		private readonly ISensorStatisticsRepository _stats;
		private readonly ISensorRepository _sensors;

		public StatisticsController(ISensorStatisticsRepository stats, ISensorRepository sensors,
			IHttpContextAccessor ctx) : base(ctx)
		{
			this._stats = stats;
			this._sensors = sensors;
		}

		[HttpGet]
		[ActionName("QueryAllStats")]
		[ProducesResponseType(typeof(IEnumerable<SensorStatisticsEntry>), 200)]
		[ProducesResponseType(403)]
		[ProducesResponseType(typeof(Status), 400)]
		public async Task<IActionResult> Index()
		{
			Task<IEnumerable<SensorStatisticsEntry>>[] workers;
			var __sensors = await this._sensors.GetAsync(this.CurrentUser).AwaitBackground();
			var sensors = __sensors.ToList();

			if(sensors.Count <= 0) {
				return this.Ok();
			}

			workers = new Task<IEnumerable<SensorStatisticsEntry>>[sensors.Count];

			for(var idx = 0; idx < sensors.Count; idx++) {
				workers[idx] = this._stats.GetBetweenAsync(sensors[idx], DateTime.MinValue, DateTime.Now.ThisHour());
			}

			var data = await Task.WhenAll(workers).AwaitBackground();
			var jobj = data.Select(Flatten).Select((flat, idx) => new {
				SensorId = sensors[idx].InternalId.ToString(),
				Statistics = flat
			}).ToList();

			return this.Ok(jobj);
		}

		public async Task<IActionResult> GetByMethod(Sensor sensor, RequestMethod method, DateTime start, DateTime end)
		{
			var data = await this._stats.GetAsync(e => e.SensorId == sensor.InternalId && e.Method == method &&
			                                           e.Date >= start && e.Date <= end).AwaitBackground();
			var flat = Flatten(data);

			foreach(var entry in flat) {
				entry.Method = method;
			}

			return this.Ok(flat);
		}

		[HttpGet("{sensorid}")]
		[ActionName("QueryStatsByDate")]
		[ProducesResponseType(typeof(IEnumerable<SensorStatisticsEntry>), 200)]
		[ProducesResponseType(403)]
		[ProducesResponseType(typeof(Status), 400)]
		public async Task<IActionResult> StatisticsBySensor(string sensorid, [FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] RequestMethod method = RequestMethod.Any)
		{
			var status = new Status {ErrorCode = ReplyCode.BadInput, Message = "Invalid request!"};

			if(string.IsNullOrEmpty(sensorid))
				return this.BadRequest(status);

			var sensor = await this._sensors.GetAsync(sensorid).AwaitBackground();

			if(sensor == null)
				return this.BadRequest(status);

			if(sensor.Owner != this.CurrentUser.Id)
				return this.Forbid();

			if(end == DateTime.MinValue)
				end = DateTime.Now;

			if(method != RequestMethod.Any)
				return await this.GetByMethod(sensor, method, start, end).AwaitBackground();

			var data = await this._stats.GetBetweenAsync(sensor, start, end).AwaitBackground();
			return this.Ok(Flatten(data));
		}

		private const  int DaysPerWeek = 7;

		[HttpGet("cumulative/daily")]
		public async Task<IActionResult> CumulativePerDay([FromQuery] DateTime start, [FromQuery] DateTime end)
		{
			var jobj = new JArray();
			var statistics = await this.GetStatsFor(this.CurrentUser, start, end).AwaitBackground();
			var entries = statistics.GroupBy(entry => entry.Date)
				.Select(grp => new {DayOfWeek = (int) grp.Key.DayOfWeek, Count = AccumulateStatisticsEntries(grp.AsEnumerable())}).ToList();

			for(var idx = 0; idx < DaysPerWeek; idx++) {
				var entry = entries.Where(e => e.DayOfWeek == idx).ToList();
				var count = entry.Aggregate(0L, (current, value) => current + value.Count);

				jobj.Add(JToken.FromObject(new {
					dayOfTheWeek = idx,
					measurements = count
				}));
			}

			return this.Ok(jobj);
		}

		[HttpGet("{sensorid}/cumulative/daily")]
		[ProducesResponseType(typeof(IEnumerable<DailyStatisticsEntry>), 200)]
		[ProducesResponseType(403)]
		[ProducesResponseType(typeof(Status), 400)]
		public async Task<IActionResult> CumulativePerDay(string sensorid, [FromQuery] DateTime start, [FromQuery] DateTime end)
		{
			var status = new Status {ErrorCode = ReplyCode.BadInput, Message = "Invalid request!"};
			var jobj = new List<DailyStatisticsEntry>();

			if(string.IsNullOrEmpty(sensorid))
				return this.BadRequest(status);

			var sensor = await this._sensors.GetAsync(sensorid).AwaitBackground();

			if(sensor == null)
				return this.BadRequest(status);

			if(sensor.Owner != this.CurrentUser.Id)
				return this.Forbid();

			if(end == DateTime.MinValue)
				end = DateTime.Now;

			var statistics = await this._stats.GetBetweenAsync(sensor, start, end).AwaitBackground();
			var entries = statistics.GroupBy(entry => entry.Date)
				.Select(grp => new {DayOfWeek = (int) grp.Key.DayOfWeek, Count = AccumulateStatisticsEntries(grp.AsEnumerable())}).ToList();

			for(var idx = 0; idx < DaysPerWeek; idx++) {
				var entry = entries.Where(e => e.DayOfWeek == idx).ToList();
				var count = entry.Aggregate(0L, (current, value) => current + value.Count);

				jobj.Add(new DailyStatisticsEntry {
					DayOfTheWeek = idx,
					Measurements = count
				});
			}

			return this.Ok(jobj);
		}

		[HttpGet("cumulative")]
		public async Task<IActionResult> Cumulative([FromQuery] DateTime start, [FromQuery] DateTime end)
		{
			var statistics = await this.GetStatsFor(this.CurrentUser, start, end).AwaitBackground();
			long counter = 0;
			IDictionary<DateTime, long> totals = new Dictionary<DateTime, long>();

			var grouped = statistics.GroupBy(entry => entry.Date).Select(grp => new {
				Timestamp = grp.Key,
				Count = grp.AsEnumerable().Aggregate(0L, (current, entry) => current + entry.Measurements)
			}).ToList();

			foreach(var entry in grouped) {
				counter += entry.Count;
				totals[entry.Timestamp] = counter;
			}

			return this.Ok(totals.Select(kvp => new {
				Timestamp = kvp.Key,
				Measurements = kvp.Value
			}));
		}

		[HttpGet("{sensorid}/cumulative")]
		public async Task<IActionResult> Cumulative(string sensorid, [FromQuery] DateTime start, [FromQuery] DateTime end)
		{
			long counter = 0;
			IDictionary<DateTime, long> totals = new Dictionary<DateTime, long>();
			var status = new Status {ErrorCode = ReplyCode.BadInput, Message = "Invalid request!"};

			if(string.IsNullOrEmpty(sensorid))
				return this.BadRequest(status);

			var sensor = await this._sensors.GetAsync(sensorid).AwaitBackground();

			if(sensor == null)
				return this.BadRequest(status);

			if(sensor.Owner != this.CurrentUser.Id)
				return this.Forbid();

			if(end == DateTime.MinValue)
				end = DateTime.Now;

			var statistics = await this._stats.GetBetweenAsync(sensor, start, end).AwaitBackground();

			var grouped = statistics.GroupBy(entry => entry.Date).Select(grp => new {
				Timestamp = grp.Key,
				Count = grp.AsEnumerable().Aggregate(0L, (current, entry) => current + entry.Measurements)
			}).ToList();

			foreach(var entry in grouped) {
				counter += entry.Count;
				totals[entry.Timestamp] = counter;
			}

			return this.Ok(totals.Select(kvp => new {
				Timestamp = kvp.Key,
				Measurements = kvp.Value
			}));
		}

		private async Task<IEnumerable<SensorStatisticsEntry>> GetStatsFor(SensateUser user, DateTime start, DateTime end)
		{
			var raw = await this._sensors.GetAsync(user).AwaitBackground();
			var sensors = raw.ToList();
			List<SensorStatisticsEntry> rv;

			if(sensors.Count <= 0)
				return null;

			var tasks = new Task<IEnumerable<SensorStatisticsEntry>>[sensors.Count];
			for(var i = 0; i < sensors.Count; i++) {
				tasks[i] = this._stats.GetBetweenAsync(sensors[i], start, end);
			}

			var results = await Task.WhenAll(tasks).AwaitBackground();
			rv = new List<SensorStatisticsEntry>();

			foreach(var entry in results) {
				rv.AddRange(entry);
			}

			return rv;
		}

		private static IEnumerable<SensorStatisticsEntry> Flatten(IEnumerable<SensorStatisticsEntry> data)
		{
			var sorted = data.GroupBy(entry => entry.Date).Select(grp => grp.AsEnumerable());
			IList<SensorStatisticsEntry> stats = new List<SensorStatisticsEntry>();

			foreach(var entries in sorted) {
				var new_entry = new SensorStatisticsEntry();
				var stats_entries = entries as SensorStatisticsEntry[] ?? entries.ToArray();

				var first = stats_entries.FirstOrDefault();

				if(first == null)
					continue;

				new_entry.Measurements = stats_entries.Aggregate(0, (current, entry) => current + entry.Measurements);
				new_entry.Date = first.Date;
				new_entry.SensorId = first.SensorId;
				stats.Add(new_entry);
			}

			return stats;
		}

		private static long AccumulateStatisticsEntries(IEnumerable<SensorStatisticsEntry> entries)
		{
			return entries.Aggregate(0L, (current, entry) => current + entry.Measurements);
		}
	}
}
