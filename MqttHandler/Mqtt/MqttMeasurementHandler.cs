/*
 * MQTT message handler.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Events;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.In;
using SensateService.Services;

namespace SensateService.MqttHandler.Mqtt
{
	public class MqttMeasurementHandler : Middleware.MqttHandler, IDisposable
	{
		private readonly ISensorRepository sensors;
		private readonly IMeasurementRepository measurements;
		private readonly IServiceProvider provider;
		private readonly ISensorStatisticsRepository stats;
		private readonly IMqttPublishService client;
		private readonly MqttServiceOptions mqttopts;
		private readonly MeasurementStorageEventHandler _handler;

		private bool disposed;

		public MqttMeasurementHandler(ISensorRepository sensors,
									  IMeasurementRepository measurements,
									  ISensorStatisticsRepository stats,
									  IOptions<MqttServiceOptions> options,
									  IServiceProvider provider, IMqttPublishService client)
		{
			this.sensors = sensors;
			this.measurements = measurements;
			this.stats = stats;
			this.client = client;
			this.mqttopts = options.Value;
			this.provider = provider;
			this._handler = new MeasurementStorageEventHandler(options, provider, client);

			this.measurements.MeasurementReceived += this._handler.MeasurementReceived_Handler;
			this.measurements.MeasurementReceived += this._handler.InternalMqttMeasurementPublish_Handler;
#if DEBUG
			this.measurements.MeasurementReceived += this._handler.MeasurementReceived_DebugHandler;
#endif
			this.disposed = false;
		}

		public override void OnMessage(string topic, string msg)
		{
			Task t;

			t = this.OnMessageAsync(topic, msg);
			t.RunSynchronously();
		}

		public override async Task OnMessageAsync(string topic, string message)
		{
			Sensor sensor;
			RawMeasurement raw;

			if(this.disposed)
				throw new ObjectDisposedException("MeasurementHandler");

			try {
				raw = JsonConvert.DeserializeObject<RawMeasurement>(message);

				if(raw.CreatedById == null)
					return;

				sensor = await this.sensors.GetAsync(raw.CreatedById).AwaitSafely();

				await this.measurements.ReceiveMeasurementAsync(sensor, raw);
				await this.stats.IncrementAsync(sensor);
			} catch(Exception ex) {
				Console.WriteLine($"Error: {ex.Message}");
				Console.WriteLine($"Received a buggy MQTT message: {message}");
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if(this.disposed)
				return;

			if(disposing) {
#if DEBUG
				this.measurements.MeasurementReceived -= this._handler.MeasurementReceived_DebugHandler;
#endif
				this.measurements.MeasurementReceived -= this._handler.MeasurementReceived_Handler;
				this.measurements.MeasurementReceived -= this._handler.InternalMqttMeasurementPublish_Handler;
			}

			this.disposed = true;
			this._handler.Cancelled = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
