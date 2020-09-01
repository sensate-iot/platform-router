/*
 * Measurement authorization handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SensateService.Common.Data.Dto.Authorization;
using SensateService.Common.Data.Dto.Protobuf;
using SensateService.Crypto;
using SensateService.Services;

namespace SensateService.Infrastructure.Authorization
{
	public class MeasurementAuthorizationHandler : AbstractAuthorizationHandler<JsonMeasurement>
	{
		private readonly IHashAlgorithm m_algo;
		private readonly IDataCache m_cache;
		private readonly IMqttPublishService m_publisher;

		private const int SecretSubStringOffset = 3;
		private const int SecretSubStringStart = 1;

		public MeasurementAuthorizationHandler(IHashAlgorithm algo, IDataCache cache, IMqttPublishService publisher)
		{
			this.m_algo = algo;
			this.m_cache = cache;
			this.m_publisher = publisher;
		}

		public override Task ProcessAsync()
		{
			List<JsonMeasurement> measurements;
			MeasurementData data;

			this.m_lock.Lock();

			try {
				var newList = new List<JsonMeasurement>();
				measurements = this.m_messages;
				this.m_messages = newList;
			} finally {
				this.m_lock.Unlock();
			}

			if(measurements == null) {
				return Task.CompletedTask;
			}

			data = new MeasurementData();
			measurements = measurements.OrderBy(m => m.Measurement.SensorId).ToList();
			Sensor sensor = null;

			foreach(var measurement in measurements) {
				if(sensor == null || sensor.Id != measurement.Measurement.SensorId) {
					sensor = this.m_cache.GetSensor(measurement.Measurement.SensorId);
				}

				if(sensor == null || !this.AuthorizeMessage(measurement, sensor)) {
					continue;
				}

				var m = new Common.Data.Dto.Protobuf.Measurement {
					SensorId = sensor.Id.ToString(),
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

				data.Measurements.Add(m);
			}

			return Task.CompletedTask;
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
