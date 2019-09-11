/*
 * Cached measurement store implementation. This store acts as
 * an insert cache for (raw) measurements. The store is entirely
 * thread-safe.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using Newtonsoft.Json;

using SensateService.Enums;
using SensateService.Exceptions;
using SensateService.Helpers;
using SensateService.Infrastructure.Events;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.In;

namespace SensateService.Infrastructure.Storage
{
	using RawMeasurementEntry = Tuple<RequestMethod, RawMeasurement>;

	public class CachedMeasurementStore : AbstractMeasurementStore, ICachedMeasurementStore
	{
		public static event OnMeasurementsReceived MeasurementsReceived;

		private List<RawMeasurementEntry> _measurements;
		private int _count;
		private SpinLockWrapper _lock;
		private readonly IMemoryCache _cache;

		private const int InitialListSize = 512;
		private const int DatabaseTimeout = 20;

		public CachedMeasurementStore(IServiceProvider provider, ILogger<CachedMeasurementStore> logger) :
			base(provider, logger)
		{
			this._lock = new SpinLockWrapper();
			this._cache = provider.GetRequiredService<IMemoryCache>();

			this._lock.Lock();
			this._measurements = new List<RawMeasurementEntry>(InitialListSize);
			this._count = 0;
			this._lock.Unlock();
		}

		public override Task StoreAsync(RawMeasurement obj, RequestMethod method)
		{
			if(obj.CreatedById == null || obj.CreatedBySecret == null)
				return Task.CompletedTask;

			this._lock.Lock();
			this._measurements.Add(new RawMeasurementEntry(method, obj));
			this._count += 1;
			this._lock.Unlock();

			return Task.CompletedTask;
		}

		public async Task StoreRangeAsync(IEnumerable<RawMeasurement> measurements, RequestMethod method)
		{
			await Task.Run(() => {
				var data = measurements.Select(m => new RawMeasurementEntry(method, m)).ToList();

				this._lock.Lock();
				this._measurements.AddRange(data);
				this._count += data.Count;
				this._lock.Unlock();
			}).AwaitBackground();
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
				if(!this._cache.TryGetValue(id, out var obj)) {
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

				opts.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
				opts.SetSize(1);
				this._cache.Set(user.Id, user, opts);
			}

			return map;
		}

		private async Task<IList<ProcessedMeasurement>> ProcessMeasurementsAsync(ICollection<RawMeasurementEntry> logs)
		{
			IList<ProcessedMeasurement> measurements;
			IList<string> sensors_ids;
			IDictionary<string, SensateUser> users;
			IDictionary<string, Sensor> sensors;

			sensors_ids = logs.Select(raw => raw.Item2.CreatedById).Distinct().ToList();
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
					Sensor sensor;
					SensateUser user;

					sensor = sensors[raw.Item2.CreatedById];

					if(sensor == null)
						continue;

					user = users[sensor.Owner];

					if(!base.CanInsert(user) || !base.InsertAllowed(user, raw.Item2.CreatedBySecret))
						continue;

					measurement = base.ProcessRawMeasurement(sensor, raw.Item2);

					if(measurement == null)
						continue;

					processed = new ProcessedMeasurement(measurement, sensor) {
						Method = raw.Item1
					};

					measurements.Add(processed);
				}
			}

			return measurements;
		}

		private static async Task IncrementStatistics(ISensorStatisticsRepository stats, IEnumerable<ProcessedMeasurement> data, CancellationToken token)
		{
			var sorted = from entry in data
				group entry by new { entry.Creator, entry.Method }
				into g
				select new {
					Sensor = g.Key.Creator,
					Length = g.ToList().Count,
					RequestMethod = g.Key.Method
				};

			var ary = sorted.ToArray();
			var tasks = new Task[ary.Length];

			for(var idx = 0; idx < ary.Length; idx++) {
				var entry = ary[idx];
				tasks[idx] = stats.IncrementManyAsync(entry.Sensor, entry.RequestMethod, entry.Length, token);
			}

			await Task.WhenAll(tasks).AwaitBackground();
		}

		private static string Compress(IList<LiveDataMeasurementWrapper> measurements)
		{
			string rv;
			MemoryStream msi, mso;
			GZipStream gzip;

			msi = null;
			mso = null;

			try {
				var data = JsonConvert.SerializeObject(measurements);
				var bytes = Encoding.UTF8.GetBytes(data);

				msi = new MemoryStream(bytes);
				mso = new MemoryStream();
				gzip = new GZipStream(mso, CompressionMode.Compress);

				msi.CopyTo(gzip);
				gzip.Dispose();

				rv = Convert.ToBase64String(mso.ToArray());
			} finally {
				mso?.Dispose();
				msi?.Dispose();
			}

			return rv;
		}

		public async Task<long> ProcessAsync()
		{
			List<RawMeasurementEntry> raw_que;
			IList<ProcessedMeasurement> processed_que;
			int count;

			this._lock.Lock();

			if(this._count <= 0L) {
				this._lock.Unlock();
				return 0;
			}

			raw_que = new List<RawMeasurementEntry>(this._count);

			this.SwapQueues(ref raw_que);
			this._lock.Unlock();

			using(var scope = this.Provider.CreateScope()) {
				var measurements = scope.ServiceProvider.GetRequiredService<IMeasurementRepository>();
				var statistics = scope.ServiceProvider.GetRequiredService<ISensorStatisticsRepository>();
				var source = new CancellationTokenSource(TimeSpan.FromSeconds(DatabaseTimeout));

				try {
					IDictionary<Sensor, List<Measurement>> telemetry;
					IList<LiveDataMeasurementWrapper> livedata;
					var io = new Task[3];

					processed_que = await this.ProcessMeasurementsAsync(raw_que).AwaitBackground();
					io[0] = IncrementStatistics(statistics, processed_que, source.Token);

					if(processed_que.Count <= 0)
						return 0;

					telemetry = processed_que.GroupBy(x => x.Creator)
						.ToDictionary(x => x.Key, v => v.Select(x => x.Measurement)
							.OrderBy(x => x.CreatedAt).ToList());
					livedata = telemetry.Select(kv => new LiveDataMeasurementWrapper(kv.Key.InternalId, kv.Value))
						.ToList();

					io[1] = measurements.StoreAsync(telemetry, source.Token);
					io[2] = this.InvokeEventHandlersAsync(livedata, source.Token);
					await Task.WhenAll(io).AwaitBackground();

					count = processed_que.Count;
				} catch(DatabaseException ex) {
					source.Cancel(false);

					this.Logger.LogInformation($"Measurement database error: {ex.Message}");
					this.Logger.LogDebug(ex.StackTrace);

					throw new DatabaseException("CachedMeasurementStore failed to store measurements", ex.Database, ex.InnerException);
				} catch(Exception ex) {
					source.Cancel(false);

					this.Logger.LogInformation($"Bulk measurement I/O error: {ex.Message}");
					this.Logger.LogDebug(ex.StackTrace);

					throw new CachingException("Unable to store measurements or statistics!", "MeasurementCache", ex);
				}
			}

			processed_que.Clear();
			raw_que.Clear();

			return count;
		}

		private async Task InvokeEventHandlersAsync(IList<LiveDataMeasurementWrapper> measurements, CancellationToken token)
		{
			Delegate[] delegates;
			MeasurementsReceivedEventArgs args;

			if(MeasurementsReceived == null)
				return;

			delegates = MeasurementsReceived.GetInvocationList();

			if(delegates.Length <= 0)
				return;

			var task = Task.Run(() => Compress(measurements), token);

			args = new MeasurementsReceivedEventArgs(token) {
				Compressed = await task.AwaitBackground()
			};

			await MeasurementsReceived.Invoke(this, args).AwaitBackground();
		}

		private void SwapQueues(ref List<RawMeasurementEntry> data)
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
		public RequestMethod Method { get; set; }

		public ProcessedMeasurement(Measurement measurement, Sensor creator)
		{
			this.Measurement = measurement;
			this.Creator = creator;
			this.Method = RequestMethod.Any;
		}
	}

	internal class LiveDataMeasurementWrapper
	{
		public List<Measurement> Measurements { get; }
		public ObjectId CreatedBy { get; }

		public LiveDataMeasurementWrapper(ObjectId id, List<Measurement> data)
		{
			this.CreatedBy = id;
			this.Measurements = data;
		}
	}
}
