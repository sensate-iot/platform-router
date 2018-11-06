/**
 * Admin control API.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Auth.Json;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models.Json.In;
using SensateService.Models.Json.Out;

namespace SensateService.Auth.Controllers
{
	[AdministratorUser]
	[Produces("application/json")]
	[Route("[controller]")]
	public class AdminController : AbstractController
	{
		private const int DaysPerWeek = 7;
		private const int HoursPerDay = 24;

		private readonly ISensorStatisticsRepository _stats;
		private readonly ISensorRepository _sensors;

		public AdminController(IUserRepository users, IAuditLogRepository audit,
			ISensorStatisticsRepository stats, ISensorRepository sensors) : base(users, audit)
		{
			this._stats = stats;
			this._sensors = sensors;
		}

		[HttpPost("find")]
		[ProducesResponseType(typeof(List<User>), 200)]
		public async Task<IActionResult> Find([FromBody] SearchQuery query)
		{
			List<User> users;
			var result = await this._users.FindByEmailAsync(query.Query).AwaitSafely();

			await this.Log(RequestMethod.HttpPost, this.CurrentUser).AwaitSafely();
			users = result.Select(user => new User {
				Email = user.Email,
				FirstName = user.FirstName,
				LastName = user.LastName,
				PhoneNumber = user.PhoneNumber,
				Id = user.Id,
				RegisteredAt = user.RegisteredAt.ToUniversalTime()
			}).ToList();

			return new OkObjectResult(users);
		}

		[HttpGet]
		[ProducesResponseType(typeof(AdminDashboard), 200)]
		public async Task<IActionResult> Index()
		{
			AdminDashboard db;
			long measurementCount;

			await this.Log(RequestMethod.HttpPost, this.CurrentUser).AwaitSafely();

			var regworker = this.GetRegistrations();
			var usercount = this._users.CountAsync();
			var ghosts = this._users.CountGhostUsersAsync();
			var measurements = await this._stats.GetAfterAsync(DateTime.Now);
			var measurementStats = this.GetMeasurementStats();
			var sensors = this._sensors.CountAsync();

			measurementCount = measurements.Aggregate(0L, (current, entry) => current + entry.Measurements);

			db = new AdminDashboard {
				Registrations = await regworker.AwaitSafely(),
				NumberOfUsers = await usercount.AwaitSafely(),
				NumberOfGhosts = await ghosts.AwaitSafely(),
				MeasurementStatsLastHour = measurementCount,
				MeasurementStats = await measurementStats.AwaitSafely(),
				NumberOfSensors = await sensors.AwaitSafely()
			};

			return this.Ok(db.ToJson());
		}

		private async Task<Graph<DateTime, long>> GetMeasurementStats()
		{
			DateTime today;
			Graph<DateTime, long> graph;
			Dictionary<long, long> totals;

			today = DateTime.Now.ToUniversalTime().Date;
			graph = new Graph<DateTime, long>();
			totals = new Dictionary<long, long>();

			var measurements = await this._stats.GetAfterAsync(today).AwaitSafely();
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
			var registrations = await this._users.CountByDay(lastweek).AwaitSafely();

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
