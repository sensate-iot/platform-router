/*
 * Websocket handler used to receive messages.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SensateService.Enums;
using SensateService.Exceptions;
using SensateService.Helpers;
using SensateService.Infrastructure.Events;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.In;
using SensateService.Services;

namespace SensateService.WebSocketHandler
{
	public class WebSocketMeasurementHandler : Middleware.WebSocketHandler
	{
		private readonly IMqttPublishService client;
		private readonly MqttServiceOptions mqttopts;
		private readonly IServiceProvider provider;
		private bool disposed;

		public WebSocketMeasurementHandler(IWebSocketRepository sockets,
										   IMqttPublishService client,
										   IServiceProvider provider,
									       IOptions<MqttServiceOptions> options) : base(sockets)
		{
			this.provider = provider;
			this.client = client;
			this.disposed = false;
			this.mqttopts = options.Value;

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

			msg = e.Measurement.ToJson();
			await this.client.PublishOnAsync(this.mqttopts.InternalMeasurementTopic, msg, false);
		}

		private async Task MeasurementReceived_Handler(object sender, MeasurementReceivedEventArgs e)
		{
			SensateUser user;
			AuditLog log;

			if(this.disposed)
				throw new ObjectDisposedException("MeasurementHandler");

			if(!(sender is Sensor sensor))
				return;

			try {
				using(var scope = this.provider.CreateScope()) {
					var sp = scope.ServiceProvider;
					var users = sp.GetRequiredService<IUserRepository>();
					var auditlogs = sp.GetRequiredService<IAuditLogRepository>();

					user = users.Get(sensor.Owner);
					log = new AuditLog {
						Address = IPAddress.Any,
						Method = RequestMethod.MqttTcp,
						Route = "NA",
						Timestamp = DateTime.Now,
						Author = user
					};

					await auditlogs.CreateAsync(log, e.CancellationToken);
				}
			} catch (Exception ex) {
				Console.WriteLine(ex.Message);
				Console.WriteLine("");
				Console.WriteLine(ex.StackTrace);
			}
		}

		public override async Task Receive(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
		{
			string msg;
			RawMeasurement raw;
			Sensor sensor;
			IMeasurementRepository measurements;
			ISensorRepository sensors;
			ISensorStatisticsRepository stats;

			msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
			measurements = null;

			try {
				using(var scope = this.provider.CreateScope()) {
					var sp = scope.ServiceProvider;
					measurements = sp.GetRequiredService<IMeasurementRepository>();
					sensors = sp.GetRequiredService<ISensorRepository>();
					stats = sp.GetRequiredService<ISensorStatisticsRepository>();

					measurements.MeasurementReceived += MeasurementReceived_Handler;
					measurements.MeasurementReceived += InternalMqttMeasurementPublish_Handler;
#if DEBUG
					measurements.MeasurementReceived += MeasurementReceived_DebugHandler;
#endif

				}

				raw = JsonConvert.DeserializeObject<RawMeasurement>(msg);

				if(raw.CreatedById == null)
					return;

				sensor = await sensors.GetAsync(raw.CreatedById).AwaitSafely();

				await measurements.ReceiveMeasurementAsync(sensor, raw);
				await stats.IncrementAsync(sensor);
			} catch(InvalidRequestException ex) {
				Debug.WriteLine($"Unable to store measurement: {ex.Message}");
				dynamic jobj = new JObject();
				jobj.status = ex.ErrorCode;
				await this.SendMessage(socket, jobj.ToString());
			} catch(Exception ex) {
				Debug.WriteLine($"Unable to store measurement: {ex.Message}");
				dynamic jobj = new JObject();
				jobj.status = 500;
				await this.SendMessage(socket, jobj.ToString());
			} finally {
				if(measurements != null) {
					measurements.MeasurementReceived -= MeasurementReceived_Handler;
					measurements.MeasurementReceived -= InternalMqttMeasurementPublish_Handler;
#if DEBUG
					measurements.MeasurementReceived -= MeasurementReceived_DebugHandler;
#endif
				}
			}
		}
	}
}
