/*
 * MQTT message handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Events;
using SensateService.Infrastructure.Storage;
using SensateService.Models;
using SensateService.Services;
using SensateService.Services.Settings;

namespace SensateService.MqttHandler.Mqtt
{
	public class MqttRealTimeMeasurementHandler : Middleware.MqttHandler, IDisposable
	{
		private readonly IMqttPublishService client;
		private readonly InternalMqttServiceOptions mqttopts;
		private readonly IMeasurementStore store;
		private readonly ILogger<MqttRealTimeMeasurementHandler> m_Logger;

		private bool disposed;

		public MqttRealTimeMeasurementHandler(IMeasurementStore store,
			ILogger<MqttRealTimeMeasurementHandler> logger,
			IOptions<InternalMqttServiceOptions> options, IMqttPublishService client)
		{
			this.client = client;
			this.mqttopts = options.Value;
			this.store = store;
			this.m_Logger = logger;

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

			this.m_Logger.LogDebug($"Received measurement from {{{sensor.Name}}}:{{{sensor.InternalId}}}");
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
			if(this.disposed) {
				throw new ObjectDisposedException("MeasurementHandler");
			}

			try {
				await this.store.StoreAsync(message, RequestMethod.MqttTcp).AwaitBackground();
			} catch(Exception ex) {
				this.m_Logger.LogInformation($"Error: {ex.Message}");
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
