/*
 * MQTT background service
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Diagnostics;

using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Channel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using SensateService.Models;
using SensateService.Infrastructure.Repositories;

namespace SensateService.Services
{
	public class MqttService
	{
		private IMqttClient _client;
		private readonly IMqttClientOptions _opts;
		private readonly MqttOptions _options;
		private readonly ILogger<MqttService> _logger;

		private readonly ISensorRepository sensors;
		private readonly IMeasurementRepository measurements;

		public MqttService(ISensorRepository srepo, IMeasurementRepository mrepo, MqttOptions options)
		{
			var factory = new MqttFactory();
			MqttClientOptionsBuilder builder;

			this._options = options;
			this.sensors = srepo;
			this.measurements = mrepo;

			this._client = factory.CreateMqttClient();
			builder = new MqttClientOptionsBuilder()
				.WithClientId(options.Id)
				.WithTcpServer(options.Host, options.Port)
				.WithCleanSession();

			if(options.Ssl) {
				builder.WithTls(true);
			}

			if(options.Username != null) {
				builder.WithCredentials(options.Username, options.Password);
			}

			this._opts = builder.Build();
			this._logger = new LoggerFactory().CreateLogger<MqttService>();

			this._client.Connected += OnConnect_Handler;
			this._client.Disconnected += OnDisconnect_Handler;
			this._client.ApplicationMessageReceived += OnMessage_Handler;
		}

		public async Task<bool> ConnectAsync()
		{
			this._logger.LogInformation($"Connecting to MQTT broker");
			Debug.WriteLine("Connecting to MQTT broker");

			await this._client.ConnectAsync(this._opts);
			return _client.IsConnected;
		}

		public async void OnMessage_Handler(
			object sender,
			MqttApplicationMessageReceivedEventArgs e
		)
		{
			Sensor sensor;
			string json, id;

			json = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
			try {
				dynamic obj = JObject.Parse(json);
				id = obj.CreatedById;
				if(id == null)
					return;

				sensor = await this.sensors.GetAsync(id);
				await this.measurements.ReceiveMeasurement(sensor, json);
			} catch(Exception) {
				Debug.WriteLine("Buggy MQTT message received!\n");
			}
		}

		private async void OnConnect_Handler(
			object sender,
			MqttClientConnectedEventArgs e
		)
		{
			var tf = new TopicFilterBuilder()
				.WithTopic(this._options.Topic)
				.WithExactlyOnceQoS()
				.Build();

			await this._client.SubscribeAsync(tf);
			Debug.WriteLine("--- MQTT client connected ---");
			this._logger.LogInformation("--- MQTT client connected ---");
		}

		private async void OnDisconnect_Handler(
			object sender,
			MqttClientDisconnectedEventArgs e
		)
		{
			Debug.WriteLine("--- MQTT client disconnected ---");
			this._logger.LogInformation("--- MQTT client disconnected ---");

			await Task.Delay(TimeSpan.FromSeconds(5D));
			try {
				await this._client.ConnectAsync(this._opts);
			} catch {
				Debug.WriteLine("--- MQTT client unable to reconnect ---");
				this._logger.LogInformation("Unable to reconnect");
			}
		}
	}
}
