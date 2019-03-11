/*
 * Cached measurement store implementation. This store acts as
 * an insert cache for (raw) measurements. The store is entirely
 * thread-safe.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Events;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.In;

namespace SensateService.Infrastructure.Storage
{
	using MeasurementLog = Tuple<RawMeasurement, AuditLog>;

	public class CachedMeasurementStore : AbstractMeasurementStore, ICachedMeasurementStore
	{
		public static event OnMeasurementsReceived MeasurementsReceived;

		private List<MeasurementLog> _measurements;
		private int _count;
		private SpinLockWrapper _lock;
		private readonly IMemoryCache cache;

		private const string AuditLogRoute = "sensate/measurements/new";
		private const int InitialListSize = 512;

		public CachedMeasurementStore(IServiceProvider provider, ILogger<CachedMeasurementStore> logger) :
			base(provider, logger)
		{
			this._lock = new SpinLockWrapper();
			this.cache = provider.GetRequiredService<IMemoryCache>();

			this._lock.Lock();
			this._measurements = new List<MeasurementLog>(InitialListSize);
			this._count = 0;
			this._lock.Unlock();
		}

		public override Task StoreAsync(RawMeasurement obj, RequestMethod method)
		{
			var log = new AuditLog {
				Address = IPAddress.Any,
				Method = method,
				Route = AuditLogRoute,
				Timestamp = DateTime.Now.ToUniversalTime(),
			};

			return Task.Run(() => {
					this._lock.Lock();

					this._measurements.Add(new MeasurementLog(obj, log));
					this._count += 1;

					this._lock.Unlock();
			});
		}

		public Task StoreRangeAsync(IEnumerable<RawMeasurement> measurements, RequestMethod method)
		{
			var logs = measurements.Select(measurement => {
				var log = new AuditLog {
					Address = IPAddress.Any,
					Method = method,
					Route = AuditLogRoute,
					Timestamp = DateTime.Now.ToUniversalTime(),
				};
				return new MeasurementLog(measurement, log);
			}).ToList();

			return Task.Run(() => {
				this._lock.Lock();
				this._measurements.AddRange(logs);
				this._count += logs.Count;
				this._lock.Unlock();
			});
		}

		private static async Task<Sensor[]> FetchDistinctSensors(ISensorRepository repo, IEnumerable<string> ids)
		{
			var raw_sensors = await repo.GetAsync(ids).AwaitBackground();
			return raw_sensors.ToArray();
		}

		private async Task<IDictionary<string, SensateUser>> GetUserInformation(IUserRepository users, IEnumerable<string> ids)
		{
			var unkowns = new List<string>();
			var map = new Dictionary<string, SensateUser>();

			foreach(var id in ids) {
				if(!cache.TryGetValue(id, out var obj)) {
					unkowns.Add(id);
					continue;
				}

				if(!(obj is SensateUser user))
					throw new KeyNotFoundException("Value is not of type SensateUser");

				map[id] = user;
			}

			if(unkowns.Count <= 0)
				return map;

			var userdata = await users.GetRangeAsync(unkowns).AwaitBackground();

			foreach(var user in userdata) {
				map[user.Id] = user;
				var opts = new MemoryCacheEntryOptions();

				opts.SetSlidingExpiration(TimeSpan.FromHours(1));
				opts.SetSize(1);
				cache.Set(user.Id, user, opts);
			}

			return map;
		}

		private async Task<IList<ProcessedMeasurement>> ProcessMeasurementsAsync(ICollection<MeasurementLog> logs)
		{
			IList<ProcessedMeasurement> measurements;
			IList<string> sensors_ids;
			IDictionary<string, SensateUser> users;
			IDictionary<string, Sensor> sensors;

			sensors_ids = logs.Select(raw => raw.Item1.CreatedById).Distinct().ToList();
			measurements = new List<ProcessedMeasurement>(logs.Count);

			using(var scope = this.Provider.CreateScope()) {
				var sensorsrepo = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
				var usersrepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

				var raw_sensors = await FetchDistinctSensors(sensorsrepo, sensors_ids).AwaitBackground();
				var distinct_users = raw_sensors.Select(sensor => sensor.Owner).Distinct();

				users = await GetUserInformation(usersrepo, distinct_users).AwaitBackground();
				sensors = raw_sensors.ToDictionary(key => key.InternalId.ToString(), sensor => sensor);

				foreach(var raw in logs) {
					ProcessedMeasurement processed;
					Measurement measurement;
					SensateUser user;

					var sensor = sensors[raw.Item1.CreatedById];
					measurement = base.ProcessRawMeasurement(sensor, raw.Item1);

					if(sensor == null)
						continue;

					user = users[sensor.Owner];
					raw.Item2.AuthorId = user.Id;

					if(!base.CanInsert(user) || measurement == null)
						continue;

					processed = new ProcessedMeasurement(measurement, sensor);
					measurements.Add(processed);
				}
			}

			return measurements;
		}

		private static Task<IList<Measurement>> SortMeasurementsAsync(IEnumerable<ProcessedMeasurement> data)
		{
			return Task.Run(() => {
				var measurements = data.Select(entry => entry.Measurement);
				var sorted = measurements.OrderByDescending(measurement => measurement.CreatedAt);

				return sorted.ToList() as IList<Measurement>;
			});
		}

		private static Task<IList<AuditLog>> SortAuditLogsAsync(IEnumerable<MeasurementLog> data)
		{
			return Task.Run(() => {
				var logs = data.Select(obj => obj.Item2);
				var sorted = logs.OrderByDescending(log => log.Timestamp);

				return sorted.ToList() as IList<AuditLog>;
			});
		}

		private static async Task IncrementStatistics(ISensorStatisticsRepository stats,
			IEnumerable<ProcessedMeasurement> data, CancellationToken token)
		{
			var sorted = from entry in data
				group entry by entry.Creator
				into g
				select new {Sensor = g.Key, Length = g.ToList().Count};

			var ary = sorted.ToArray();
			var tasks = new Task[ary.Length];

			for(var idx = 0; idx < ary.Length; idx++) {
				var entry = ary[idx];
				tasks[idx] = stats.IncrementManyAsync(entry.Sensor, entry.Length, token);
			}

			await Task.WhenAll(tasks).AwaitBackground();
		}

		public async Task<long> ProcessAsync()
		{
			IList<ProcessedMeasurement> processed_que;
			List<MeasurementLog> raw_que;
			int count;

			this._lock.Lock();
			count = this._count;

			if(count <= 0L) {
				this._lock.Unlock();
				return 0;
			}

			raw_que = new List<MeasurementLog>(count);

			this.SwapQueues(ref raw_que);
			this._lock.Unlock();

			using(var scope = this.Provider.CreateScope()) {
				var measurements = scope.ServiceProvider.GetRequiredService<IBulkWriter<Measurement>>();
				var logs = scope.ServiceProvider.GetRequiredService<IBulkWriter<AuditLog>>();
				var stats = scope.ServiceProvider.GetRequiredService<ISensorStatisticsRepository>();
				var source = new CancellationTokenSource();

				try {
					var workers = new Task[4];
					Task<IList<AuditLog>> log_task;
					Task<IList<Measurement>> measurements_task;

					/*
					 * Generate measurements using the raw measurements as input. Invalid
					 * raw measurements are silently discarded. Both measurements and audit logs
					 * are then stored asynchronously in bulk. An audit log entry is created even
					 * if the raw measurement was discarded.
					 */

					processed_que = await this.ProcessMeasurementsAsync(raw_que).AwaitBackground();
					log_task = SortAuditLogsAsync(raw_que);
					measurements_task = SortMeasurementsAsync(processed_que);
					await Task.WhenAll(measurements_task, log_task).AwaitBackground();

					workers[0] = measurements.CreateRangeAsync(measurements_task.Result, source.Token);
					workers[1] = logs.CreateRangeAsync(log_task.Result, source.Token);
					workers[2] = InvokeEventHandlersAsync(this, measurements_task.Result, source.Token);
					workers[3] = IncrementStatistics(stats, processed_que, source.Token);

					await Task.WhenAll(workers).AwaitBackground();
				} catch(Exception ex) {
					source.Cancel(false);

					this.Logger.LogInformation($"Bulk measurement I/O error: {ex.Message}");
					this.Logger.LogInformation(ex.StackTrace);

					return 0;
				}
			}

			processed_que.Clear();
			raw_que.Clear();

			return count;
		}

		private static Task InvokeEventHandlersAsync(object sender, IList<Measurement> measurements, CancellationToken token)
		{
			Delegate[] delegates;
			MeasurementsReceivedEventArgs args;

			if(MeasurementsReceived == null)
				return Task.CompletedTask;

			delegates = MeasurementsReceived.GetInvocationList();

			if(delegates.Length <= 0)
				return Task.CompletedTask;

			args = new MeasurementsReceivedEventArgs(measurements, token);
			return MeasurementsReceived.Invoke(sender, args);
		}

		private void SwapQueues(ref List<MeasurementLog> data)
		{
			var tmp_m = this._measurements;

			this._measurements = data;
			data = tmp_m;
			this._count = 0;
		}

		public void Destroy()
		{
			this._lock.Lock();

			this._measurements.Clear();
			this._count = 0;

			this._lock.Unlock();
		}
	}

	internal class ProcessedMeasurement
	{
		public Measurement Measurement { get; }
		public Sensor Creator { get; }

		public ProcessedMeasurement(Measurement measurement, Sensor creator)
		{
			this.Measurement = measurement;
			this.Creator = creator;
		}
	}
}
