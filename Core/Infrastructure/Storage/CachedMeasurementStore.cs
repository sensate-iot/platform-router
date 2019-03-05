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

		private const string AuditLogRoute = "sensate/measurements/new";
		private const int InitialListSize = 512;

		public CachedMeasurementStore(IServiceProvider provider, ILogger<CachedMeasurementStore> logger) :
			base(provider, logger)
		{
			this._lock = new SpinLockWrapper();

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
				Timestamp = DateTime.Now,
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
					Timestamp = DateTime.Now,
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

		private static async Task<IList<SensateUser>> FetchDistinctUsers(IUserRepository repo, IEnumerable<Sensor> sensors)
		{
			var distinct_users = sensors.Select(sensor => sensor.Owner).Distinct();
			var rawusers = await repo.GetRangeAsync(distinct_users).AwaitBackground();
			return rawusers.ToList();
		}

		private static async Task<Sensor[]> FetchDistinctSensors(ISensorRepository repo, IEnumerable<string> ids)
		{
			var raw_sensors = await repo.GetAsync(ids).AwaitBackground();
			return raw_sensors.ToArray();
		}

		private async Task<IList<ProcessedMeasurement>> ProcessMeasurementsAsync(ICollection<MeasurementLog> logs)
		{
			IList<ProcessedMeasurement> measurements;
			IList<string> sensors_ids;
			IDictionary<string, SensateUser> users;
			IDictionary<string, Sensor> sensors;
			IDictionary<string, string[]> roles;

			sensors_ids = logs.Select(raw => raw.Item1.CreatedById).Distinct().ToList();
			measurements = new List<ProcessedMeasurement>(logs.Count);
			roles = new Dictionary<string, string[]>();

			using(var scope = this.Provider.CreateScope()) {
				var sensorsrepo = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
				var usersrepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

				var raw_sensors = await FetchDistinctSensors(sensorsrepo, sensors_ids).AwaitBackground();
				var raw_users = await FetchDistinctUsers(usersrepo, raw_sensors).AwaitBackground();

				users = raw_users.ToDictionary(key => key.Id, user => user);
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

					if(!roles.TryGetValue(sensor.Owner, out var userroles)) {
						var rawroles = await usersrepo.GetRolesAsync(user).AwaitBackground();
						userroles = rawroles.ToArray();
						roles[user.Id] = userroles;
					}

					if(!base.CanInsert(userroles) || measurement == null)
						continue;

					processed = new ProcessedMeasurement(measurement, sensor);
					measurements.Add(processed);
				}
			}

			return measurements;
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
					IList<Measurement> tmp;
					IList<AuditLog> log_que;

					/*
					 * Generate measurements using the raw measurements as input. Invalid
					 * raw measurements are silently discarded. Both measurements and audit logs
					 * are then stored asynchronously in bulk. An audit log entry is created even
					 * if the raw measurement was discarded.
					 */

					processed_que = await this.ProcessMeasurementsAsync(raw_que).AwaitBackground();
					log_que = raw_que.Select(log => log.Item2).ToList();
					tmp = processed_que.Select(m => m.Measurement).ToList();

					workers[0] = measurements.CreateRangeAsync(tmp, source.Token);
					workers[1] = logs.CreateRangeAsync(log_que, source.Token);
					workers[2] = InvokeEventHandlersAsync(this, tmp, source.Token);
					workers[3] = Task.Run(async () => {
						var sorted = from entry in processed_que 
							group entry by entry.Creator
							into g
							select new {Sensor = g.Key, Length = g.ToList().Count};

						var ary = sorted.ToArray();
						var tasks = new Task[ary.Length];

						for(var idx = 0; idx < ary.Length; idx++) {
							var entry = ary[idx];
							tasks[idx] = stats.IncrementManyAsync(entry.Sensor, entry.Length);
						}

						await Task.WhenAll(tasks).AwaitBackground();
					}, source.Token);

					await Task.WhenAll(workers).AwaitBackground();
					tmp.Clear();
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
