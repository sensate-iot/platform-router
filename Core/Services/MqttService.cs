/*
 * MQTT background service
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MQTTnet;
using MQTTnet.Client;

using SensateService.Infrastructure.Repositories;
using SensateService.Middleware;

namespace SensateService.Services
{
	public class MqttService : BackgroundService
	{
		private readonly IServiceProvider _provider;
		private readonly ConcurrentDictionary<string, Type> _handlers;
		private readonly MqttServiceOptions options;
		private readonly ILogger<MqttService> _logger;

		private IMqttClient _client;
		private IMqttClientOptions _client_options;

		public MqttService(IServiceProvider provider,
						   IOptions<MqttServiceOptions> options,
						   ILogger<MqttService> logger)
		{
			this._provider = provider;
			this._handlers = new ConcurrentDictionary<string, Type>();
			this.options = options.Value;
			this._logger = logger;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			var factory = new MqttFactory();
			MqttClientOptionsBuilder builder;

			this._client = factory.CreateMqttClient();
			builder = new MqttClientOptionsBuilder()
				.WithClientId(this.options.Id)
				.WithTcpServer(this.options.Host, this.options.Port)
				.WithCleanSession();

			if(this.options.Ssl) {
				builder.WithTls(true);
			}

			if(this.options.Username != null) {
				builder.WithCredentials(this.options.Username, this.options.Password);
			}

			this._client.Connected += OnConnect_Handler;
			this._client.Disconnected += OnDisconnect_HandlerAsync;
			this._client.ApplicationMessageReceived += OnMessage_Handler;

			this._client_options = builder.Build();
			await this.Connect();
		}

		private async Task Connect()
		{
			this._logger.LogInformation($"Connecting to MQTT broker");
			Debug.WriteLine("Connecting to MQTT broker");

			await this._client.ConnectAsync(this._client_options);
		}

		public async void OnMessage_Handler(
			object sender,
			MqttApplicationMessageReceivedEventArgs e
		)
		{
			MqttHandler handler;
			Type handlerType;
			string msg;

			using(var scope = this._provider.CreateScope()) {
				if(!this._handlers.TryGetValue(e.ApplicationMessage.Topic, out handlerType)) {
					this._handlers.TryGetValue(
						this.options.TopicShare + e.ApplicationMessage.Topic,
						out handlerType
					);
				}

				handler = scope.ServiceProvider.GetRequiredService(handlerType) as MqttHandler;
				msg = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
				await handler.OnMessageAsync(e.ApplicationMessage.Topic, msg);
			}
		}

		private void OnConnect_Handler(
			object sender,
			MqttClientConnectedEventArgs e
		)
		{
			TopicFilterBuilder tfb;
			List<TopicFilter> filters;

			filters = new List<TopicFilter>();

			foreach(var tuple in this._handlers) {
				tfb = new TopicFilterBuilder().WithAtMostOnceQoS();
				tfb.WithTopic(tuple.Key);
				filters.Add(tfb.Build());
			}

			this._client.SubscribeAsync(filters);
			Debug.WriteLine("--- MQTT client connected ---");
			this._logger.LogInformation("--- MQTT client connected ---");
		}

		private async void OnDisconnect_HandlerAsync(
			object sender,
			MqttClientDisconnectedEventArgs e
		)
		{
			Debug.WriteLine("--- MQTT client disconnected ---");
			this._logger.LogInformation("--- MQTT client disconnected ---");

			await Task.Delay(TimeSpan.FromSeconds(5D));
			try {
				await this._client.ConnectAsync(this._client_options);
			} catch {
				Debug.WriteLine("--- MQTT client unable to reconnect ---");
				this._logger.LogInformation("Unable to reconnect");
			}
		}

		public void MapTopicHandler<T>(string topic) where T : MqttHandler
		{
			this._handlers.TryAdd(topic, typeof(T));
		}
	}
}
