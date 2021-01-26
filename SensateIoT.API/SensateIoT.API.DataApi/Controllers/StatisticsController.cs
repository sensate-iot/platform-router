/*
 * Measurement statistics repository.
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
using Microsoft.Extensions.Logging;

using MongoDB.Bson;

using SensateIoT.API.Common.ApiCore.Attributes;
using SensateIoT.API.Common.ApiCore.Controllers;
using SensateIoT.API.Common.Core.Helpers;
using SensateIoT.API.Common.Core.Infrastructure.Repositories;
using SensateIoT.API.Common.Data.Dto.Json.Out;
using SensateIoT.API.Common.Data.Enums;
using SensateIoT.API.Common.Data.Models;
using SensateIoT.API.Common.IdentityData.Models;
using SensateIoT.API.DataApi.Json;

using SensorStatisticsEntry = SensateIoT.API.Common.Data.Models.SensorStatisticsEntry;

namespace SensateIoT.API.DataApi.Controllers
{
	[Produces("application/json")]
	[Route("data/v1/[controller]")]
	public class StatisticsController : AbstractDataController
	{
		private readonly ISensorStatisticsRepository _stats;
		private readonly ISensorRepository _sensors;
		private readonly IAuditLogRepository m_auditlogs;
		private readonly ITriggerRepository m_triggers;
		private readonly ILogger<StatisticsController> m_logger;
		private readonly IUserRepository m_users;
		private readonly IMessageRepository m_messages;
		private readonly IBlobRepository m_blobs;
		private readonly IControlMessageRepository m_control;

		public StatisticsController(ISensorStatisticsRepository stats,
									ISensorRepository sensors,
									ISensorLinkRepository links,
									IAuditLogRepository logs,
									IUserRepository users,
									ITriggerRepository triggers,
									IControlMessageRepository control,
									IMessageRepository messages,
									IBlobRepository blobs,
									IApiKeyRepository keys,
									ILogger<StatisticsController> loger,
									IHttpContextAccessor ctx) : base(ctx, sensors, links, keys)
		{
			this._stats = stats;
			this._sensors = sensors;
			this.m_logger = loger;
			this.m_auditlogs = logs;
			this.m_triggers = triggers;
			this.m_users = users;
			this.m_blobs = blobs;
			this.m_control = control;
			this.m_messages = messages;
		}

		[HttpGet(Name = "StatsIndex")]
		[ActionName("QueryAllStats")]
		[ProducesResponseType(typeof(IEnumerable<StatisticsEntry>), 200)]
		[ProducesResponseType(200)]
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
			var jobj = data.Select(Flatten).Select((flat, idx) => {
				var entry = new StatisticsEntry { SensorId = sensors[idx].InternalId.ToString(), Statistics = flat };
				return entry;
			}).ToList();

			return this.Ok(jobj);
		}

		private async Task<IActionResult> GetByMethod(Sensor sensor, RequestMethod method, DateTime start, DateTime end)
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
		[ProducesResponseType(typeof(Status), 403)]
		[ProducesResponseType(typeof(Status), 400)]
		public async Task<IActionResult> StatisticsBySensor(string sensorid, [FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] RequestMethod method = RequestMethod.Any)
		{
			var status = new Status { ErrorCode = ReplyCode.BadInput, Message = "Invalid request!" };

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

		private const int DaysPerWeek = 7;

		[HttpGet("cumulative/daily")]
		[ProducesResponseType(typeof(IEnumerable<DailyStatisticsEntry>), 200)]
		[ProducesResponseType(typeof(Status), 403)]
		[ProducesResponseType(typeof(Status), 400)]
		public async Task<IActionResult> CumulativePerDay([FromQuery] DateTime start, [FromQuery] DateTime end)
		{
			var jobj = new List<DailyStatisticsEntry>();
			var statistics = await this.GetStatsFor(this.CurrentUser, start, end).AwaitBackground();
			var entries = statistics.GroupBy(entry => entry.Date)
				.Select(grp => new { DayOfWeek = (int)grp.Key.DayOfWeek, Count = AccumulateStatisticsEntries(grp.AsEnumerable()) }).ToList();

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

		[HttpGet("{sensorid}/cumulative/daily")]
		[ProducesResponseType(typeof(IEnumerable<DailyStatisticsEntry>), 200)]
		[ProducesResponseType(403)]
		[ProducesResponseType(typeof(Status), 403)]
		[ProducesResponseType(typeof(Status), 400)]
		public async Task<IActionResult> CumulativePerDay(string sensorid, [FromQuery] DateTime start, [FromQuery] DateTime end)
		{
			var status = new Status { ErrorCode = ReplyCode.BadInput, Message = "Invalid request!" };
			var jobj = new List<DailyStatisticsEntry>();

			if(string.IsNullOrEmpty(sensorid)) {
				return this.BadRequest(status);
			}

			var sensor = await this._sensors.GetAsync(sensorid).AwaitBackground();

			if(sensor == null) {
				return this.BadRequest(status);
			}

			if(sensor.Owner != this.CurrentUser.Id) {
				return this.Forbid();
			}

			if(end == DateTime.MinValue) {
				end = DateTime.Now;
			}

			var statistics = await this._stats.GetBetweenAsync(sensor, start, end).AwaitBackground();
			var entries = statistics.GroupBy(entry => entry.Date)
				.Select(grp => new { DayOfWeek = (int)grp.Key.DayOfWeek, Count = AccumulateStatisticsEntries(grp.AsEnumerable()) }).ToList();

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

		[HttpGet("count/{userId}")]
		[ProducesResponseType(typeof(Count), 200)]
		[ProducesResponseType(typeof(Status), 400)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[AdminApiKey]
		public async Task<IActionResult> Count(string userId,
											   [FromQuery] string sensorId,
											   [FromQuery] DateTime start,
											   [FromQuery] DateTime end)
		{

			var user = await this.m_users.GetAsync(userId).AwaitBackground();
			return await this.CountAsync(user, sensorId, start, end).AwaitBackground();
		}

		[HttpGet("count")]
		[ProducesResponseType(typeof(Count), 200)]
		[ProducesResponseType(typeof(Status), 403)]
		[ProducesResponseType(typeof(Status), 400)]
		[ProducesResponseType(403)]
		[ProducesResponseType(404)]
		public async Task<IActionResult> Count([FromQuery] string sensorId,
											   [FromQuery] DateTime start,
											   [FromQuery] DateTime end)
		{
			return await this.CountAsync(this.CurrentUser, sensorId, start, end).AwaitBackground();
		}

		private async Task<IActionResult> CountAsync(SensateUser user,
											   string sensorId,
											   DateTime start,
											   DateTime end)
		{
			IList<Sensor> sensors;
			Count count;

			try {

				if(user == null) {
					return this.NotFound();
				}

				if(!string.IsNullOrEmpty(sensorId)) {
					var s = await this._sensors.GetAsync(sensorId).AwaitBackground();

					if(s == null) {
						return this.NotFound();
					}

					if(!await this.AuthenticateUserForSensor(s, false).AwaitBackground()) {
						return this.Forbid();
					}

					sensors = new List<Sensor> { s };
				} else {
					var tmp = await this._sensors.GetAsync(user).AwaitBackground();
					sensors = tmp.ToList();
				}

				var messages = this.m_messages.CountAsync(sensors, start, end);
				var actuators = this.m_control.CountAsync(sensors, start, end);
				var blobTask = this.m_blobs.GetAsync(sensors, start, end);
				var result = await this._stats.GetBetweenAsync(sensors, start, end).AwaitBackground();

				var blobs = await blobTask.AwaitBackground();
				var bytes = blobs.Aggregate(0L, (x, blob) => x + blob.FileSize);

				var aggregated = result.Aggregate(0L, (r, item) => r + item.Measurements);
				var logs = await this.m_auditlogs.CountAsync(entry => entry.AuthorId == user.Id &&
														  entry.Timestamp >= start.ToUniversalTime() &&
														  entry.Timestamp <= end.ToUniversalTime() &&
														  (entry.Method == RequestMethod.HttpGet ||
														   entry.Method == RequestMethod.MqttWebSocket ||
														   entry.Method == RequestMethod.HttpDelete ||
														   entry.Method == RequestMethod.HttpPatch ||
														   entry.Method == RequestMethod.HttpPost ||
														   entry.Method == RequestMethod.HttpPut)).AwaitBackground();

				var invocationCount = await this.m_triggers.CountAsync(sensors.Select(x => x.InternalId), default)
					.ConfigureAwait(false);

				count = new Count {
					BlobStorage = bytes,
					Sensors = await this._sensors.CountAsync(user).AwaitBackground(),
					Measurements = aggregated,
					Links = await this.m_links.CountAsync(user).AwaitBackground(),
					TriggerInvocations = invocationCount,
					ApiCalls = logs,
					Messages = await messages.AwaitBackground(),
					ActuatorMessages = await actuators.AwaitBackground()
				};
			} catch(Exception ex) {
				this.m_logger.LogDebug(ex, $"Unable to count statistics between {start} and {end}");
				this.m_logger.LogInformation($"Unable to count statistics between {start} and {end}");

				return this.BadRequest(new Status {
					Message = "Unable to count statistics",
					ErrorCode = ReplyCode.BadInput
				});
			}

			return this.Ok(count);
		}

		[HttpGet("cumulative")]
		[ProducesResponseType(typeof(MeasurementsAtDateTime), 200)]
		[ProducesResponseType(typeof(Status), 403)]
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

			return this.Ok(totals.Select(kvp => new MeasurementsAtDateTime {
				Timestamp = kvp.Key,
				Measurements = kvp.Value
			}));
		}

		[HttpGet("{sensorid}/cumulative")]
		[ProducesResponseType(typeof(MeasurementsAtDateTime), 200)]
		[ProducesResponseType(typeof(Status), 403)]
		[ProducesResponseType(403)]
		[ProducesResponseType(typeof(Status), 400)]
		public async Task<IActionResult> Cumulative(string sensorid, [FromQuery] DateTime start, [FromQuery] DateTime end)
		{
			long counter = 0;
			IDictionary<DateTime, long> totals = new Dictionary<DateTime, long>();
			var status = new Status { ErrorCode = ReplyCode.BadInput, Message = "Invalid request!" };

			if(string.IsNullOrEmpty(sensorid))
				return this.BadRequest(status);

			var sensor = await this._sensors.GetAsync(sensorid).AwaitBackground();

			if(sensor == null) {
				return this.BadRequest(status);
			}

			if(sensor.Owner != this.CurrentUser.Id) {
				return this.Forbid();
			}

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

			return this.Ok(totals.Select(kvp => new MeasurementsAtDateTime {
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

				if(first == null) {
					continue;
				}

				new_entry.InternalId = ObjectId.GenerateNewId();
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
