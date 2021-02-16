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
using Microsoft.Extensions.DependencyInjection;

using Google.Protobuf.WellKnownTypes;

using SensateIoT.Platform.Network.API.Abstract;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.API.DTO;
using SensateIoT.Platform.Network.API.Services;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.Models;

using DataPoint = SensateIoT.Platform.Network.Contracts.DTO.DataPoint;
using Measurement = SensateIoT.Platform.Network.Contracts.DTO.Measurement;

namespace SensateIoT.Platform.Network.API.Authorization
{
	public class MeasurementAuthorizationService : AbstractAuthorizationHandler<JsonMeasurement>, IMeasurementAuthorizationService
	{
		private readonly IHashAlgorithm m_algo;
		private readonly IRouterClient m_router;
		private readonly ILogger<MeasurementAuthorizationService> m_logger;
		private readonly IServiceProvider m_provider;

		public MeasurementAuthorizationService(IServiceProvider provider,
											   IHashAlgorithm algo,
											   IRouterClient client,
											   ILogger<MeasurementAuthorizationService> logger)
		{
			this.m_logger = logger;
			this.m_algo = algo;
			this.m_provider = provider;
			this.m_router = client;
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

			measurements = measurements.OrderBy(m => m.Item1.SensorId).ToList();
			var data = await this.BuildMeasurementList(measurements).ConfigureAwait(false);
			var remaining = data.Count;
			var index = 0;
			var tasks = new List<Task>();

			if(remaining <= 0) {
				return 0;
			}

			while(remaining > 0) {
				var batch = Math.Min(PartitionSize, remaining);

				tasks.Add(this.SendBatchToRouterAsync(index, remaining, data));
				index += batch;
				remaining -= batch;
			}

			await Task.WhenAll(tasks).ConfigureAwait(false);
			return data.Count;
		}

		private async Task SendBatchToRouterAsync(int start, int count, IList<Measurement> measurements)
		{
			var data = new MeasurementData();

			for(var idx = start; idx < start + count; idx++) {
				var measurement = measurements[idx];
				data.Measurements.Add(measurement);
			}

			var result = await this.m_router.RouteAsync(data, default).ConfigureAwait(false);
			this.m_logger.LogInformation("Sent {inputCount} messages to the router. {outputCount} " +
										 "messages have been accepted. Router response ID: {responseID}.",
										 data.Measurements.Count, result.Count, new Guid(result.ResponseID.Span));
		}

		private async Task<IList<Measurement>> BuildMeasurementList(IEnumerable<JsonMeasurement> measurements)
		{
			Sensor sensor = null;
			var data = new List<Measurement>();

			using var scope = this.m_provider.CreateScope();
			var repo = scope.ServiceProvider.GetRequiredService<ISensorRepository>();

			foreach(var measurement in measurements) {
				try {
					if(sensor == null || sensor.InternalId != measurement.Item1.SensorId) {
						sensor = await repo.GetAsync(measurement.Item1.SensorId).ConfigureAwait(false);
					}

					if(sensor == null || !this.AuthorizeMessage(measurement, sensor)) {
						continue;
					}

					if(measurement.Item1.Timestamp == DateTime.MinValue) {
						measurement.Item1.Timestamp = DateTime.UtcNow;
					}

					var m = new Measurement {
						SensorID = sensor.InternalId.ToString(),
						Latitude = decimal.ToDouble(measurement.Item1.Latitude),
						Longitude = decimal.ToDouble(measurement.Item1.Longitude),
						Timestamp = Timestamp.FromDateTime(measurement.Item1.Timestamp)
					};

					foreach(var (key, dp) in measurement.Item1.Data) {
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

			return data;
		}

		protected override bool AuthorizeMessage(JsonMeasurement measurement, Sensor sensor)
		{
			var match = this.m_algo.GetMatchRegex();
			var search = this.m_algo.GetSearchRegex();

			if(match.IsMatch(measurement.Item1.Secret)) {
				var length = measurement.Item1.Secret.Length - SecretSubStringOffset;
				var hash = HexToByteArray(measurement.Item1.Secret.Substring(SecretSubStringStart, length));
				var json = search.Replace(measurement.Item2, sensor.Secret, 1);
				var binary = Encoding.ASCII.GetBytes(json);
				var computed = this.m_algo.ComputeHash(binary);

				if(!CompareHashes(computed, hash)) {
					return false;
				}
			} else {
				return measurement.Item1.Secret == sensor.Secret;
			}

			return true;
		}
	}
}
