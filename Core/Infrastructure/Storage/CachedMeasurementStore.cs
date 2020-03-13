﻿/*
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

		private List<RawMeasurementEntry> m_measurements;
		private int m_count;
		private SpinLockWrapper m_lock;

		private const int InitialListSize = 512;
		private const int DatabaseTimeout = 20;

		public CachedMeasurementStore(IServiceProvider provider, ILogger<CachedMeasurementStore> logger) :
			base(provider, logger)
		{
			this.m_lock = new SpinLockWrapper();

			this.m_lock.Lock();
			this.m_measurements = new List<RawMeasurementEntry>(InitialListSize);
			this.m_count = 0;
			this.m_lock.Unlock();
		}

		public override Task StoreAsync(RawMeasurement obj, RequestMethod method)
		{
			if(obj.CreatedById == null || obj.CreatedBySecret == null)
				return Task.CompletedTask;

			this.m_lock.Lock();
			this.m_measurements.Add(new RawMeasurementEntry(method, obj));
			this.m_count += 1;
			this.m_lock.Unlock();

			return Task.CompletedTask;
		}

		public async Task StoreRangeAsync(IEnumerable<RawMeasurement> measurements, RequestMethod method)
		{
			await Task.Run(() => {
				var data = measurements.Select(m => new RawMeasurementEntry(method, m)).ToList();

				this.m_lock.Lock();
				this.m_measurements.AddRange(data);
				this.m_count += data.Count;
				this.m_lock.Unlock();
			}).AwaitBackground();
		}

		private static async Task<Sensor[]> FetchDistinctSensors(ISensorRepository repo, IEnumerable<string> ids)
		{
			var raw_sensors = await repo.GetAsync(ids).AwaitBackground();
			return raw_sensors.ToArray();
		}

		private static async Task<IDictionary<string, SensateUser>> GetUserInformation(IUserRepository users, IEnumerable<string> ids)
		{
			var userdata = await users.GetRangeAsync(ids).AwaitBackground();
			return userdata.ToDictionary(user => user.Id);
		}

		private ProcessedMeasurement ValidateMeasurement(IDictionary<string, SensateUser> users,
			IDictionary<string, Sensor> sensors, RawMeasurementEntry raw)
		{
			ProcessedMeasurement processed;
			Measurement measurement;

			if (!sensors.TryGetValue(raw.Item2.CreatedById, out Sensor sensor)) {
				return null;
			}

			if (!users.TryGetValue(sensor.Owner, out SensateUser user)) {
				return null;
			}

			if(!base.CanInsert(user) || !base.InsertAllowed(user, raw.Item2.CreatedBySecret)) {
				return null;
			}

			measurement = base.ProcessRawMeasurement(sensor, raw.Item2);

			if(measurement == null) {
				return null;
			}

			processed = new ProcessedMeasurement(measurement, sensor) {
				Method = raw.Item1
			};

			return processed;
		}

		private async Task<IList<ProcessedMeasurement>> ProcessMeasurementsAsync(ICollection<RawMeasurementEntry> logs)
		{
			List<ProcessedMeasurement> measurements;
			IList<string> sensors_ids;
			IDictionary<string, SensateUser> users;
			IDictionary<string, Sensor> sensors;
			IList<Task<ProcessedMeasurement>> tasks;

			sensors_ids = logs.Select(raw => raw.Item2.CreatedById).Distinct().ToList();

			using(var scope = this.Provider.CreateScope()) {
				var sensorsrepo = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
				var usersrepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

				var raw_sensors = await FetchDistinctSensors(sensorsrepo, sensors_ids).AwaitBackground();
				var distinct_users = raw_sensors.Select(sensor => sensor.Owner).Distinct();

				users = await GetUserInformation(usersrepo, distinct_users).AwaitBackground();
				sensors = raw_sensors.ToDictionary(key => key.InternalId.ToString(), sensor => sensor);
				tasks = logs.Select(raw => Task.Run(() => this.ValidateMeasurement(users, sensors, raw))).ToList();

				var data = await Task.WhenAll(tasks).AwaitBackground();
				measurements = data.ToList();
				measurements.RemoveAll(m => m == null);
			}

			return measurements;
		}

		private static async Task IncrementStatistics(ISensorStatisticsRepository stats, ICollection<StatisticsUpdate> data, CancellationToken token)
		{
			var tasks = new Task[data.Count];

			for(var idx = 0; idx < data.Count; idx++) {
				var entry = data.ElementAt(idx);
				tasks[idx] = stats.IncrementManyAsync(entry.Sensor, entry.Method, entry.Count, token);
			}

			await Task.WhenAll(tasks).AwaitBackground();
		}

		private async Task InvokeEventHandlersAsync(IList<LiveDataMeasurementWrapper> measurements, CancellationToken token)
		{
			Delegate[] delegates;
			MeasurementsReceivedEventArgs args;

			if(MeasurementsReceived == null)
				return;

			delegates = MeasurementsReceived.GetInvocationList();

			if(delegates.Length <= 0) {
				return;
			}

			var task = Task.Run(() => Compress(measurements), token);

			args = new MeasurementsReceivedEventArgs(token) {
				Compressed = await task.AwaitBackground()
			};

			await MeasurementsReceived.Invoke(this, args).AwaitBackground();
		}

		public async Task<long> ProcessMeasurementsAsync()
		{
			IList<ProcessedMeasurement> processed_q;
			long count;

			this.m_lock.Lock();

			if(this.m_count <= 0L) {
				this.m_lock.Unlock();
				return 0;
			}

			this.SwapQueues(out var raw_q);
			this.m_lock.Unlock();

			processed_q = null;

			using(var scope = this.Provider.CreateScope()) {
				var measurementsdb = scope.ServiceProvider.GetRequiredService<IMeasurementRepository>();
				var statsdb = scope.ServiceProvider.GetRequiredService<ISensorStatisticsRepository>();
				var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DatabaseTimeout));

				try {
					IDictionary<Sensor, List<Measurement>> data;
					IList<LiveDataMeasurementWrapper> livedata;
					IList<StatisticsUpdate> statsdata;
					var asyncio = new Task[3];

					processed_q = await this.ProcessMeasurementsAsync(raw_q).AwaitBackground();

					if(processed_q.Count <= 0L) {
						return 0L;
					}

					var mapped =
						from measurement in processed_q
						group measurement by new { measurement.Creator, measurement.Method }
						into g
						select new SortedMeasurementEntry {
							Creator = g.Key.Creator,
							Measurements = g.Select(m => m.Measurement).ToList(),
							Method = g.Key.Method
						};
					var sorted = mapped.ToList();

					statsdata = sorted.Select(e => new StatisticsUpdate(e.Method, e.Measurements.Count, e.Creator)).ToList();
					asyncio[0] = IncrementStatistics(statsdb, statsdata, cts.Token);

					data = sorted.ToDictionary(key => key.Creator, value => value.Measurements.ToList());
					count = data.Aggregate(0L, (x, measurements) => x + measurements.Value.Count);
					asyncio[1] = measurementsdb.StoreAsync(data, cts.Token);

					livedata = sorted.Select(x => new LiveDataMeasurementWrapper(x.Creator.InternalId, x.Measurements.ToList())).ToList();
					asyncio[2] = this.InvokeEventHandlersAsync(livedata, cts.Token);

					await Task.WhenAll(asyncio).AwaitBackground();
				} catch(DatabaseException e) {
					cts.Cancel(false);

					this.Logger.LogInformation($"Measurement database error: {e.Message}");
					this.Logger.LogDebug(e.StackTrace);

					count = 0;
				} catch(Exception e) {
					cts.Cancel(false);

					this.Logger.LogInformation($"Bulk measurement I/O error: {e.Message}");
					this.Logger.LogDebug(e.StackTrace);

					throw new CachingException("Unable to store measurements or statistics!", "MeasurementCache", e);
				} finally {
					raw_q.Clear();
					processed_q?.Clear();
				}
			}

			return count;
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

		private void SwapQueues(out List<RawMeasurementEntry> data)
		{
			data = this.m_measurements;
			this.m_measurements = new List<RawMeasurementEntry>(data.Count);
			this.m_count = 0;
		}

		public void Destroy()
		{
			this.m_lock.Lock();

			this.m_measurements.Clear();
			this.m_count = 0;

			this.m_lock.Unlock();
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

	internal class StatisticsUpdate
	{
		public int Count { get; }
		public RequestMethod Method { get; }
		public Sensor Sensor { get; }

		public StatisticsUpdate(RequestMethod method, int count, Sensor sensor)
		{
			this.Sensor = sensor;
			this.Method = method;
			this.Count = count;
		}
	}

	internal class SortedMeasurementEntry
	{
		public Sensor Creator { get; set; }
		public RequestMethod Method { get; set; }
		public IList<Measurement> Measurements { get; set; }
	}
}

