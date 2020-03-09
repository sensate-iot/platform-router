/*
 * Admin dashboard controller.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.DashboardApi.Json;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models.Json.Out;

namespace SensateService.DashboardApi.Controllers
{
	[AdministratorUser]
	[Produces("application/json")]
	[Route("dashboard/admin")]
	public class AdminDashboardController : AbstractController
	{
		private const int DaysPerWeek = 7;
		private const int HoursPerDay = 24;

		private readonly ISensorStatisticsRepository _stats;
		private readonly ISensorRepository _sensors;

		public AdminDashboardController(IUserRepository users, IHttpContextAccessor ctx,
			ISensorStatisticsRepository stats, ISensorRepository sensors) : base(users, ctx)
		{
			this._stats = stats;
			this._sensors = sensors;
		}

		[HttpGet]
		[ProducesResponseType(typeof(AdminDashboard), 200)]
		public async Task<IActionResult> Index()
		{
			AdminDashboard db;
			long measurementCount;

			var measurementStats = this.GetMeasurementStats();
			var sensors = this._sensors.CountAsync();

			db = new AdminDashboard {
				Registrations = await this.GetRegistrations().AwaitBackground(),
				NumberOfUsers = await this._users.CountAsync().AwaitBackground(),
				NumberOfGhosts = await this._users.CountGhostUsersAsync().AwaitBackground()
			};

			var measurements = await this._stats.GetAfterAsync(DateTime.Now.ThisHour()).AwaitBackground();

			measurementCount = measurements.Aggregate(0L, (current, entry) => current + entry.Measurements);

			db.MeasurementStatsLastHour = measurementCount;
			db.MeasurementStats = await measurementStats.AwaitBackground();
			db.NumberOfSensors = await sensors.AwaitBackground();

			return this.Ok(db.ToJson());
		}

		private async Task<Graph<DateTime, long>> GetMeasurementStats()
		{
			DateTime today;
			Graph<DateTime, long> graph;
			Dictionary<long, long> totals;

			today = DateTime.Now.AddHours(-23D).ToUniversalTime().ThisHour();
			graph = new Graph<DateTime, long>();
			totals = new Dictionary<long, long>();

			var measurements = await this._stats.GetAfterAsync(today).AwaitBackground();
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

		private async Task<Graph<DateTime, int>> GetRegistrations()
		{
			var now = DateTime.Now;
			Graph<DateTime, int> graph = new Graph<DateTime, int>();

			/* Include today */
			var lastweek = now.AddDays((DaysPerWeek - 1) * -1).ToUniversalTime().Date;
			var registrations = await this._users.CountByDay(lastweek).AwaitBackground();

			for(int idx = 0; idx < DaysPerWeek; idx++) {
				var entry = registrations.ElementAtOrDefault(0);

				if(entry == null || entry.Item1 > lastweek) {
					graph.Add(lastweek, 0);
				} else {
					graph.Add(lastweek, entry.Item2);
					registrations.RemoveAt(0);
				}

				lastweek = lastweek.AddDays(1D);
			}

			graph.Data.Sort();
			return graph;
		}
	}
}