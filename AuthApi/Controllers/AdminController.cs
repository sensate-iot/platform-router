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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.AuthApi.Json;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models.Json.In;
using SensateService.Models.Json.Out;

namespace SensateService.AuthApi.Controllers
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

		public AdminController(IUserRepository users, IHttpContextAccessor ctx,
			ISensorStatisticsRepository stats, ISensorRepository sensors) : base(users , ctx)
		{
			this._stats = stats;
			this._sensors = sensors;
		}

		[HttpPost("find-users")]
		[ProducesResponseType(typeof(List<User>), 200)]
		[ValidateModel]
		public async Task<IActionResult> Find([FromBody] SearchQuery query)
		{
			List<User> users;
			var result = await this._users.FindByEmailAsync(query.Query).AwaitBackground();

			users =  result.Select(user => {
				var roles = this._users.GetRoles(user);

				return new User {
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName,
					PhoneNumber = user.PhoneNumber,
					Id = user.Id,
					RegisteredAt = user.RegisteredAt.ToUniversalTime(),
					Roles = roles
				};
			}).ToList();

			return new OkObjectResult(users);
		}

		[HttpGet("users")]
		[ProducesResponseType(typeof(List<User>), 200)]
		public async Task<IActionResult> GetMostRecentUsers()
		{
			List<User> users;

			var userWorker = this._users.GetMostRecentAsync(10);
			var query = await userWorker.AwaitBackground();

			users = query.Select(user => new User {
				Email = user.Email,
				FirstName = user.FirstName,
				LastName = user.LastName,
				PhoneNumber = user.PhoneNumber,
				Id = user.Id,
				RegisteredAt = user.RegisteredAt.ToUniversalTime(),
				Roles = this._users.GetRoles(user)
			}).ToList();

			return this.Ok(users);
		}

		[HttpGet]
		[ProducesResponseType(typeof(AdminDashboard), 200)]
		public async Task<IActionResult> Index()
		{
			AdminDashboard db;
			long measurementCount;

			var regworker = this.GetRegistrations();
			var usercount = this._users.CountAsync();
			var ghosts = this._users.CountGhostUsersAsync();
			var measurements = await this._stats.GetAfterAsync(DateTime.Now.ThisHour());
			var measurementStats = this.GetMeasurementStats();
			var sensors = this._sensors.CountAsync();

			measurementCount = measurements.Aggregate(0L, (current, entry) => current + entry.Measurements);

			db = new AdminDashboard {
				Registrations = await regworker.AwaitBackground(),
				NumberOfUsers = await usercount.AwaitBackground(),
				NumberOfGhosts = await ghosts.AwaitBackground(),
				MeasurementStatsLastHour = measurementCount,
				MeasurementStats = await measurementStats.AwaitBackground(),
				NumberOfSensors = await sensors.AwaitBackground()
			};

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
