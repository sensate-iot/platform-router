/*
 * Measurement authorization handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SensateService.Common.Data.Dto.Authorization;
using SensateService.Common.Data.Dto.Protobuf;
using SensateService.Crypto;
using SensateService.Helpers;
using SensateService.Infrastructure.Authorization.Cache;
using SensateService.Infrastructure.Events;

using Measurement = SensateService.Common.Data.Dto.Protobuf.Measurement;

namespace SensateService.Infrastructure.Authorization.Handlers
{
	public class MeasurementAuthorizationHandler : AbstractAuthorizationHandler<JsonMeasurement>
	{
		private readonly IHashAlgorithm m_algo;
		private readonly IDataCache m_cache;
		private readonly ILogger<MeasurementAuthorizationHandler> m_logger;

		public MeasurementAuthorizationHandler(IHashAlgorithm algo, IDataCache cache, ILogger<MeasurementAuthorizationHandler> logger)
		{
			this.m_logger = logger;
			this.m_algo = algo;
			this.m_cache = cache;
		}

		public override async Task<int> ProcessAsync()
		{
			List<JsonMeasurement> measurements;

			this.m_lock.Lock();

			try {
				var newList = new List<JsonMeasurement>();
				measurements = this.m_messages;
				this.m_messages = newList;
			} finally {
				this.m_lock.Unlock();
			}

			if(measurements == null || measurements.Count <= 0) {
				return 0;
			}

			measurements = measurements.OrderBy(m => m.Measurement.SensorId).ToList();
			Sensor sensor = null;
			var data = new List<Measurement>();

			foreach(var measurement in measurements) {
				try {
					if(sensor == null || sensor.Id != measurement.Measurement.SensorId) {
						sensor = this.m_cache.GetSensor(measurement.Measurement.SensorId);
					}

					if(sensor == null || !this.AuthorizeMessage(measurement, sensor)) {
						continue;
					}

					if(measurement.Measurement.Timestamp == DateTime.MinValue) {
						measurement.Measurement.Timestamp = DateTime.UtcNow;
					}

					var m = new Measurement {
						SensorId = sensor.Id.ToString(),
						Latitude = decimal.ToDouble(measurement.Measurement.Latitude),
						Longitude = decimal.ToDouble(measurement.Measurement.Longitude),
						PlatformTime = DateTime.UtcNow.ToString("O"),
						Timestamp = measurement.Measurement.Timestamp.ToString("O")
					};

					foreach(var (key, dp) in measurement.Measurement.Data) {
						var datapoint = new DataPoint {
							Key = key,
							Accuracy = dp.Accuracy ?? 0,
							Precision = dp.Precision ?? 0,
							Unit = dp.Unit,
							Value = decimal.ToDouble(dp.Value)
						};

						m.Datapoints.Add(datapoint);
					}

					data.Add(m);
				} catch(Exception ex) {
					this.m_logger.LogInformation(ex, "Unable to process measurement: {message}", ex.InnerException?.Message);
				}
			}

			if(data.Count <= 0) {
				return 0;
			}

			var tasks = new List<Task>();
			var rv = data.Count;

			if(data.Count > PartitionSize) {
				var partitions = data.Partition(PartitionSize);
				var measurementData = new MeasurementData();

				foreach(var partition in partitions) {
					measurementData.Measurements.AddRange(partition);
				}

				var args = new DataAuthorizedEventArgs { Data = measurementData };

				tasks.Add(AuthorizationCache.InvokeMeasurementEvent(this, args));
			} else {
				var measurementData = new MeasurementData();
				measurementData.Measurements.AddRange(data);

				var args = new DataAuthorizedEventArgs { Data = measurementData };
				tasks.Add(AuthorizationCache.InvokeMeasurementEvent(this, args));
			}

			await Task.WhenAll(tasks).AwaitBackground();
			return rv;
		}

		protected override bool AuthorizeMessage(JsonMeasurement measurement, Sensor sensor)
		{
			var match = this.m_algo.GetMatchRegex();
			var search = this.m_algo.GetSearchRegex();

			if(match.IsMatch(measurement.Measurement.Secret)) {
				var length = measurement.Measurement.Secret.Length - SecretSubStringOffset;
				var hash = HexToByteArray(measurement.Measurement.Secret.Substring(SecretSubStringStart, length));
				var json = search.Replace(measurement.Json, sensor.Secret, 1);
				var binary = Encoding.ASCII.GetBytes(json);
				var computed = this.m_algo.ComputeHash(binary);

				if(!CompareHashes(computed, hash)) {
					return false;
				}
			} else {
				return measurement.Measurement.Secret == sensor.Secret;
			}

			return true;
		}
	}
}
