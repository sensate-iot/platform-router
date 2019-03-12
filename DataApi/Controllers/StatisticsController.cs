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
using SensateService.ApiCore.Attributes;
using SensateService.ApiCore.Controllers;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.DataApi.Controllers
{
	[NormalUser]
	[Produces("application/json")]
	[Route("[controller]")]
	public class StatisticsController : AbstractController
	{
		private readonly ISensorStatisticsRepository _stats;
		private readonly ISensorRepository _sensors;

		public StatisticsController(IUserRepository users, ISensorStatisticsRepository stats,
			ISensorRepository sensors, IHttpContextAccessor ctx) : base(users, ctx)
		{
			this._stats = stats;
			this._sensors = sensors;
		}

		[HttpGet]
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
			var jobj = data.Select(this.Flatten).Select((flat, idx) => new {
				SensorId = sensors[idx].InternalId.ToString(),
				Statistics = flat
			}).ToList();

			return this.Ok(jobj);
		}

		private IEnumerable<SensorStatisticsEntry> Flatten(IEnumerable<SensorStatisticsEntry> data)
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
	}
}
