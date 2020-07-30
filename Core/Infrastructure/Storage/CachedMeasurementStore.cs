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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json;

using SensateService.Enums;
using SensateService.Protobuf;
using SensateService.Exceptions;
using SensateService.Helpers;
using SensateService.Infrastructure.Events;
using SensateService.Infrastructure.Repositories;
using DataPoint = SensateService.Models.DataPoint;
using Measurement = SensateService.Models.Measurement;

namespace SensateService.Infrastructure.Storage
{
	public class CachedMeasurementStore : ICachedMeasurementStore, IMeasurementCache
	{
		public static event OnMeasurementsReceived MeasurementsReceived;

		private SpinLockWrapper m_lock;
		private readonly IServiceProvider m_provider;
		private List<string> m_data;
		private readonly ILogger<CachedMeasurementStore> m_logger;

		private const int InitialListSize = 512;
		private const int DatabaseTimeout = 20;

		public CachedMeasurementStore( IServiceProvider provider, ILogger<CachedMeasurementStore> logger )
		{
			this.m_provider = provider;
			this.m_logger = logger;
			this.m_lock = new SpinLockWrapper();

			this.m_lock.Lock();
			this.m_data = new List<string>(InitialListSize);
			this.m_lock.Unlock();
		}

		public Task StoreAsync(string obj, RequestMethod method)
		{
			this.m_lock.Lock();
			this.m_data.Add(obj);
			this.m_lock.Unlock();

			return Task.CompletedTask;
		}

		private static async Task IncrementStatistics(ISensorStatisticsRepository stats, ICollection<StatisticsUpdate> data, CancellationToken token)
		{
			var tasks = new Task[data.Count];

			for(var idx = 0; idx < data.Count; idx++) {
				var entry = data.ElementAt(idx);
				tasks[idx] = stats.IncrementManyAsync(entry.SensorId, entry.Method, entry.Count, token);
			}

			await Task.WhenAll(tasks).AwaitBackground();
		}

		private async Task InvokeEventHandlersAsync(IList<MeasurementQueue> measurements, CancellationToken token)
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
			long count = 0;

			this.m_lock.Lock();
			if(this.m_data.Count <= 0L) {
				this.m_lock.Unlock();
				return 0;
			}

			this.SwapQueues(out var raw_q);
			this.m_lock.Unlock();

			using var scope = this.m_provider.CreateScope();
			var measurementsdb = scope.ServiceProvider.GetRequiredService<IMeasurementRepository>();
			var statsdb = scope.ServiceProvider.GetRequiredService<ISensorStatisticsRepository>();
			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DatabaseTimeout));

			try {
				IList<MeasurementQueue> livedata;
				IList<StatisticsUpdate> statsdata;
				var asyncio = new Task[3];
				var measurements = DeflateMeasurements(raw_q);
				var measurementsDict = measurements.ToDictionary(x => x.SensorId, x => x.Measurements);

				statsdata = measurementsDict.Select(e => new StatisticsUpdate(RequestMethod.MqttTcp, e.Value.Count, e.Key)).ToList();
				asyncio[0] = IncrementStatistics(statsdb, statsdata, cts.Token);
				asyncio[1] = measurementsdb.StoreAsync(measurementsDict, cts.Token);

				livedata = measurementsDict.Select(x => new MeasurementQueue { SensorId =  x.Key, Measurements = x.Value}).ToList();
				asyncio[2] = this.InvokeEventHandlersAsync(livedata, cts.Token);

				await Task.WhenAll(asyncio).AwaitBackground();
			} catch(DatabaseException e) {
				cts.Cancel(false);

				this.m_logger.LogInformation($"Measurement database error: {e.Message}");
				this.m_logger.LogDebug(e.StackTrace);

				count = 0;
			} catch(Exception e) {
				cts.Cancel(false);

				this.m_logger.LogInformation($"Bulk measurement I/O error: {e.Message}");
				this.m_logger.LogDebug(e.StackTrace);

				throw new CachingException("Unable to store measurements or statistics!", "MeasurementCache", e);
			} finally {
				raw_q.Clear();
			}

			return count;
		}

		private static IEnumerable<MeasurementQueue> DeflateMeasurements(IList<string> data)
		{
			var rv = new List<MeasurementQueue>();

			foreach(var b64s in data) {
				var bytes = Convert.FromBase64String(b64s);
				using var to = new MemoryStream();
				using var from = new MemoryStream(bytes);
				using var gzip = new GZipStream(@from, CompressionMode.Decompress);

				gzip.CopyTo(to);
				var final = to.ToArray();
				var protoMeasurements = MeasurementData.Parser.ParseFrom(final);
				var measurements =
					from measurement in protoMeasurements.Measurements
					group measurement by measurement.SensorId into g
					select new MeasurementQueue {
						SensorId = ObjectId.Parse(g.Key),
						Measurements = g.Select(m => new Measurement {
                            Data = m.Datapoints.ToDictionary(p => p.Key, p => new DataPoint {
                                Accuracy = p.Accuracy,
                                Precision = p.Precision,
                                Unit = p.Unit,
                                Value = Convert.ToDecimal(p.Value),
                            }),
                            Location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(m.Longitude, m.Latitude)),
                            PlatformTime = DateTime.Parse(m.PlatformTime),
                            Timestamp = DateTime.Parse(m.Timestamp)
						}).ToList()
					};

				rv.AddRange(measurements);
			}

			return rv;
		}

		private static string Compress(IList<MeasurementQueue> measurements)
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

		private void SwapQueues(out List<string> data)
		{
			data = this.m_data;
			this.m_data = new List<string>(data.Count);
		}

		public void Destroy()
		{
			this.m_lock.Lock();
			this.m_data.Clear();
			this.m_lock.Unlock();
		}
	}

	internal class StatisticsUpdate
	{
		public int Count { get; }
		public RequestMethod Method { get; }
		public ObjectId SensorId { get; }

		public StatisticsUpdate(RequestMethod method, int count, ObjectId sensorId)
		{
			this.SensorId = sensorId;
			this.Method = method;
			this.Count = count;
		}
	}

	internal class MeasurementQueue
	{
		public ObjectId SensorId { get; set; }
		public List<Measurement> Measurements { get; set; }
	}
}
