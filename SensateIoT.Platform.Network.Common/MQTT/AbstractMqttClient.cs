/*
 * Abstract MQTT service. Implementation of connect and reconnect
 * handlers.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;

using SensateIoT.Platform.Network.Common.Services.Background;

namespace SensateIoT.Platform.Network.Common.MQTT
{
	public abstract class AbstractMqttClient : BackgroundService
	{
		private readonly bool _ssl;
		private readonly string _host;
		private readonly int _port;
		private readonly string _share;
		private readonly IServiceProvider _provider;
		private readonly string _id;
		private bool _disconnected;

		private readonly ConcurrentDictionary<string, Type> _handlers;

		protected IMqttClient Client { private set; get; }
		private IMqttClientOptions _client_options;
		protected readonly ILogger _logger;

		protected AbstractMqttClient(string host,
									 int port,
									 bool ssl,
									 string share,
									 string id,
									 ILogger logger,
									 IServiceProvider provider) : base(logger)
		{
			this._host = host;
			this._port = port;
			this._ssl = ssl;
			this._logger = logger;
			this._disconnected = false;
			this._provider = provider;
			this._share = share;
			this._handlers = new ConcurrentDictionary<string, Type>();
			this._id = id;

			if(string.IsNullOrEmpty(id)) {
				this._id = Guid.NewGuid().ToString();
			}
		}

		public void MapTopicHandler<T>(string topic) where T : IMqttHandler
		{
			this._handlers.TryAdd(topic, typeof(T));

			if(this.Client != null && this.Client.IsConnected && !this._disconnected && this.Client != null) {
				this.SubscribeAsync(topic).Wait();
			}
		}

		protected Task Disconnect()
		{
			this._disconnected = true;
			return this.Client.DisconnectAsync();
		}

		protected async Task Connect(string user, string passwd)
		{
			var factory = new MqttFactory();
			MqttClientOptionsBuilder builder;

			this.Client = factory.CreateMqttClient();
			builder = new MqttClientOptionsBuilder()
				.WithClientId(this._id)
				.WithTcpServer(this._host, this._port)
				.WithCleanSession();

			if(this._ssl)
				builder.WithTls(new MqttClientOptionsBuilderTlsParameters {
					AllowUntrustedCertificates = true,
					UseTls = true
				});

			if(string.IsNullOrEmpty(user)) {
				user = null;
				passwd = null;
			}

			if(user != null) {
				builder.WithCredentials(user, passwd);
			}

			this.Client.UseDisconnectedHandler(e => { this.OnDisconnect_HandlerAsync(); });
			this.Client.UseConnectedHandler(e => { this.OnConnect_Handler(); });
			this.Client.UseApplicationMessageReceivedHandler(this.OnMessage_Handler);

			this._client_options = builder.Build();
			await this.Client.ConnectAsync(this._client_options).ConfigureAwait(false);
		}

		private async Task SubscribeAsync(string topic)
		{
			var tfb = new MqttTopicFilterBuilder()
				.WithTopic(topic)
				.WithAtMostOnceQoS();
			var build = tfb.Build();
			var opts = new MqttClientSubscribeOptionsBuilder()
				.WithTopicFilter(build);

			await this.Client.SubscribeAsync(opts.Build(), CancellationToken.None);
			this._logger.LogInformation($"Subscribed to: {topic}");
		}

		protected virtual async Task OnConnectAsync()
		{
			foreach(var tuple in this._handlers) {
				await this.SubscribeAsync(tuple.Key).ConfigureAwait(false);
			}
		}

		private async Task OnMessageAsync(string topic, string msg)
		{
			IMqttHandler handler;

			using var scope = this._provider.CreateScope();
			if(!this._handlers.TryGetValue(topic, out var handlerType)) {
				this._handlers.TryGetValue(this._share + topic, out handlerType);
			}

			handler = scope.ServiceProvider.GetRequiredService(handlerType!) as IMqttHandler;

			if(handler == null)
				return;

			try {
				await handler.OnMessageAsync(topic, msg).ConfigureAwait(false);
			} catch(Exception ex) {
				this._logger.LogWarning(ex, "Unable to handle MQTT message.");
			}
		}

		private async void OnMessage_Handler(MqttApplicationMessageReceivedEventArgs e)
		{
			var msg = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
			var topic = e.ApplicationMessage.Topic;

			await this.OnMessageAsync(topic, msg).ConfigureAwait(false);
		}

		private async void OnConnect_Handler()
		{
			await this.OnConnectAsync().ConfigureAwait(false);
		}

		private async void OnDisconnect_HandlerAsync()
		{
			if(this._disconnected)
				return;

			await Task.Delay(TimeSpan.FromSeconds(5D));

			try {
				await this.Client.ConnectAsync(this._client_options);
			} catch(Exception ex) {
				this._logger.LogError(ex, "Unable to reconnect to broker: {broker}:{port}.", this._host, this._port);
				this._logger.LogInformation("Unable to reconnect");
			}
		}
	}
}
