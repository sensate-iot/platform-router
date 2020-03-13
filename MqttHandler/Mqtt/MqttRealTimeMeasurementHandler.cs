/*
 * MQTT message handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Events;
using SensateService.Infrastructure.Storage;
using SensateService.Models;
using SensateService.Models.Json.In;
using SensateService.Services;
using SensateService.Services.Settings;

namespace SensateService.MqttHandler.Mqtt
{
	public class MqttRealTimeMeasurementHandler : Middleware.MqttHandler, IDisposable
	{
		private readonly IMqttPublishService client;
		private readonly InternalMqttServiceOptions mqttopts;
		private readonly IMeasurementStore store;

		private bool disposed;

		public MqttRealTimeMeasurementHandler( IMeasurementStore store, IOptions<InternalMqttServiceOptions> options, IMqttPublishService client)
		{
			this.client = client;
			this.mqttopts = options.Value;
			this.store = store;

			MeasurementStore.MeasurementReceived += this.InternalMqttMeasurementPublish_Handler;
#if DEBUG
			MeasurementStore.MeasurementReceived += this.MeasurementReceived_DebugHandler;
#endif
			this.disposed = false;
		}

#if DEBUG
		public Task MeasurementReceived_DebugHandler(object sender, MeasurementReceivedEventArgs e)
		{
			if(!(sender is Sensor sensor))
				return Task.CompletedTask;

			Console.WriteLine($"Received measurement from {{{sensor.Name}}}:{{{sensor.InternalId}}}");
			return Task.CompletedTask;
		}
#endif

		private async Task InternalMqttMeasurementPublish_Handler(object sender, MeasurementReceivedEventArgs e)
		{
			string msg;

			var obj = new {
				e.Measurement,
				CreatedBy = e.Sensor.InternalId
			};

			msg = JsonConvert.SerializeObject(obj);
			await this.client.PublishOnAsync(this.mqttopts.InternalMeasurementTopic, msg, false);
		}

		public override void OnMessage(string topic, string msg)
		{
			Task t;

			t = this.OnMessageAsync(topic, msg);
			t.Wait();
		}

		public override async Task OnMessageAsync(string topic, string message)
		{
			RawMeasurement raw;

			if(this.disposed)
				throw new ObjectDisposedException("MeasurementHandler");

			try {
				raw = JsonConvert.DeserializeObject<RawMeasurement>(message);

				if(raw.CreatedById == null)
					return;

				await this.store.StoreAsync(raw, RequestMethod.MqttTcp).AwaitBackground();
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
				MeasurementStore.MeasurementReceived -= this.MeasurementReceived_DebugHandler;
#endif
				MeasurementStore.MeasurementReceived -= this.InternalMqttMeasurementPublish_Handler;
			}

			this.disposed = true;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
