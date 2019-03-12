/*
 * SensateDashboard index page.
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
using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.AuthApi.Json;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.Out;

namespace SensateService.AuthApi.Controllers
{
	[NormalUser]
	[Produces("application/json")]
	[Route("[controller]")]
	public class DashboardController : AbstractController
	{
		private const int HoursPerDay = 24;

		private readonly ISensorStatisticsRepository _stats;
		private readonly ISensorRepository _sensors;
		private readonly IAuditLogRepository _logs;
		private readonly IUserTokenRepository _tokens;

		public DashboardController(IUserRepository users,
			ISensorStatisticsRepository stats,
			ISensorRepository sensors,
			IUserTokenRepository tokens,
			IAuditLogRepository logs,
			IHttpContextAccessor ctx) : base(users, ctx)
		{
			this._stats = stats;
			this._sensors = sensors;
			this._logs = logs;
			this._tokens = tokens;
		}

		[HttpGet]
		[ProducesResponseType(typeof(UserDashboard), 200)]
		public async Task<IActionResult> Index()
		{
			UserDashboard board;
			var raw = await this.GetStatsFor(this.CurrentUser, DateTime.MinValue).AwaitBackground();
			var stats = raw.ToList();

			board = new UserDashboard {
				SensorCount = await this._sensors.CountAsync(this.CurrentUser).AwaitBackground(),
				ApiCallCount = await this.GetApiCallCountAsync().AwaitBackground(),
				MeasurementsTodayCount = await this.CountMeasurementsAsync().AwaitBackground(),
				SecurityTokenCount = await this.CountSecurityTokensAsync(),


				MeasurementsToday =  await this.GetMeasurementStatsToday().AwaitBackground(),
				MeasurementsCumulative =  GetMeasurementStatsCumulative(stats),
				MeasurementsPerDayCumulative = GetMeasurementStatsCumulativePerDay(stats),
				ApiCallsLastWeek = await this.GetApiCallsPerDayAsync().AwaitBackground()
			};

			return this.Ok(board.ToJson());
		}

		private async Task<long> CountSecurityTokensAsync()
		{
			var count = await this._tokens.CountAsync(token =>
					token.UserId == this.CurrentUser.Id && token.Valid && token.ExpiresAt >= DateTime.Now)
				.AwaitBackground();
			return count;
		}

		private async Task<long> CountMeasurementsAsync()
		{
			var start = DateTime.Now.Date;

			var logs = await this._logs.CountAsync(entry => entry.AuthorId == this.CurrentUser.Id &&
			                                                entry.Timestamp >= start &&
			                                                entry.Timestamp <= DateTime.Now &&
			                                                (entry.Route == "sensate/measurements/new" ||
			                                                 entry.Route == "sensate/measurements/rt/new")).AwaitBackground();
			return logs;
		}

		private async Task<long> GetApiCallCountAsync()
		{
			var start = DateTime.Now.AddMonths(-1);
			var logs = await this._logs.CountAsync(entry => entry.AuthorId == this.CurrentUser.Id &&
			                                              entry.Timestamp >= start &&
			                                              entry.Timestamp <= DateTime.Now &&
			                                              (entry.Method == RequestMethod.HttpGet ||
			                                               entry.Method == RequestMethod.HttpDelete ||
			                                               entry.Method == RequestMethod.HttpPatch ||
			                                               entry.Method == RequestMethod.HttpPost ||
			                                               entry.Method == RequestMethod.HttpPut)).AwaitBackground();
			return logs;
		}

		private async Task<IEnumerable<SensorStatisticsEntry>> GetStatsFor(SensateUser user, DateTime date)
		{
			var raw = await this._sensors.GetAsync(user).AwaitBackground();
			var sensors = raw.ToList();
			List<SensorStatisticsEntry> rv;

			if(sensors.Count <= 0)
				return null;

			var tasks = new Task<IEnumerable<SensorStatisticsEntry>>[sensors.Count];
			for(var i = 0; i < sensors.Count; i++) {
				tasks[i] = this._stats.GetAfterAsync(sensors[i], date);
			}

			var results = await Task.WhenAll(tasks).AwaitBackground();
			rv = new List<SensorStatisticsEntry>();

			foreach(var entry in results) {
				rv.AddRange(entry);
			}

			return rv;
		}

		private async Task<Graph<DateTime, long>> GetApiCallsPerDayAsync()
		{
			Graph<DateTime, long> graph;
			DateTime lastweek = DateTime.Now.Date.AddDays(-DaysPerWeek + 1);
			var start = lastweek;

			var logs = await this._logs.GetAsync(entry => entry.AuthorId == this.CurrentUser.Id &&
			                                              entry.Timestamp >= start &&
			                                              entry.Timestamp <= DateTime.Now &&
			                                              (entry.Method == RequestMethod.HttpGet ||
			                                               entry.Method == RequestMethod.HttpDelete ||
			                                               entry.Method == RequestMethod.HttpPatch ||
			                                               entry.Method == RequestMethod.HttpPost ||
			                                               entry.Method == RequestMethod.HttpPut)).AwaitBackground();
			var data = logs.GroupBy(log => log.Timestamp.Date).Select(grp => new {
				DayOfMonth = grp.Key,
				Count = grp.AsEnumerable().Count()
			}).ToList();

			graph = new Graph<DateTime, long>();

			for(var idx = 0; idx < DaysPerWeek; lastweek = lastweek.AddDays(1), idx++) {
				var count = data.SingleOrDefault(x => x.DayOfMonth == lastweek);

				if(count == null) {
					graph.Add(lastweek, 0L);
					continue;
				}

				graph.Add(count.DayOfMonth, count.Count);
			}

			return graph;
		}

		private const int DaysPerWeek = 7;

		private static long AccumulateStatisticsEntries(IEnumerable<SensorStatisticsEntry> entries)
		{
			return entries.Aggregate(0L, (current, entry) => current + entry.Measurements);
		}

		private static Graph<int, long> GetMeasurementStatsCumulativePerDay(IEnumerable<SensorStatisticsEntry> statistics)
		{
			Graph<int, long> data;

			var entries = statistics.GroupBy(entry => entry.Date)
				.Select(grp => new {DayOfWeek = (int) grp.Key.DayOfWeek, Count = AccumulateStatisticsEntries(grp.AsEnumerable())}).ToList();

			data = new Graph<int, long>();

			for(var idx = 0; idx < DaysPerWeek; idx++) {
				var entry = entries.Where(e => e.DayOfWeek == idx).ToList();
				var count = entry.Aggregate(0L, (current, value) => current + value.Count);

				data.Add(idx, count);
			}

			return data;
		}

		private static Graph<DateTime, long> GetMeasurementStatsCumulative(IEnumerable<SensorStatisticsEntry> statistics)
		{
			Graph<DateTime, long> graph;
			Dictionary<DateTime, long> totals;
			long counter;

			graph = new Graph<DateTime, long>();
			totals = new Dictionary<DateTime, long>();
			counter = 0L;

			var grouped = statistics.GroupBy(entry => entry.Date).Select(grp => new {
				grp.Key.Date,
				Count = grp.AsEnumerable().Aggregate(0L, (current, entry) => current + entry.Measurements)
			}).ToList();

			foreach(var entry in grouped) {
				counter += entry.Count;
				totals[entry.Date] = counter;
			}

			var sorted = totals.Keys.AsEnumerable().OrderBy(value => value).ToArray();

			if(sorted.Length <= 0)
				return graph;

			graph.Add(sorted[0].Date.AddDays(-1), 0L);

			foreach(var key in sorted) {
				graph.Add(key, totals[key]);
			}

			return graph;
		}

		private async Task<Graph<DateTime, long>> GetMeasurementStatsToday()
		{
			DateTime today;
			Graph<DateTime, long> graph;
			Dictionary<long, long> totals;

			today = DateTime.Now.AddHours(-23D).ToUniversalTime().ThisHour();
			graph = new Graph<DateTime, long>();
			totals = new Dictionary<long, long>();
			var measurements = await this.GetStatsFor(this.CurrentUser, today).AwaitBackground();

			foreach(var entry in measurements) {
				if(!totals.TryGetValue(entry.Date.Ticks, out var value))
					value = 0L;

				value += entry.Measurements;
				totals[entry.Date.Ticks] = value;
			}

			for(var idx = 0; idx < HoursPerDay; idx++) {
				if(!totals.TryGetValue(today.Ticks, out var value))
					value = 0L;

				graph.Add(today, value);
				today = today.AddHours(1D);
			}

			return graph;
		}
	}
}