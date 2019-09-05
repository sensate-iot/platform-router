/*
 * Abstract MQTT service. Implementation of connect and reconnect
 * handlers.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;

using SensateService.Helpers;

namespace SensateService.Services.Processing
{
	public abstract class AbstractMqttService : BackgroundService
	{
		private readonly bool _ssl;
		private readonly string _host;
		private readonly int _port;
		private bool _disconnected;

		protected IMqttClient Client { private set; get; }
		private IMqttClientOptions _client_options;
		protected readonly ILogger _logger;

		protected AbstractMqttService(string host, int port, bool ssl, ILogger logger)
		{
			this._host = host;
			this._port = port;
			this._ssl = ssl;
			this._logger = logger;
			this._disconnected = false;
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
				.WithClientId(Guid.NewGuid().ToString())
				.WithTcpServer(this._host, this._port)
				.WithCleanSession();

			if(this._ssl)
				builder.WithTls(new MqttClientOptionsBuilderTlsParameters {
					AllowUntrustedCertificates = true,
					UseTls = true
				});

			if(user != null)
				builder.WithCredentials(user, passwd);

			this.Client.UseDisconnectedHandler(e => { this.OnDisconnect_HandlerAsync(this, e); });
			this.Client.UseConnectedHandler(e => { this.OnConnect_Handler(this.Client, e); });
			this.Client.UseApplicationMessageReceivedHandler(e => this.OnMessage_Handler(this.Client, e));
			//this.Client.Connected += OnConnect_Handler;
			//this.Client.Disconnected += OnDisconnect_HandlerAsync;
			//this.Client.ApplicationMessageReceived += OnMessage_Handler;

			this._client_options = builder.Build();
			await this.Client.ConnectAsync(this._client_options).AwaitBackground();
		}

		protected abstract Task OnConnectAsync();
		protected abstract Task OnMessageAsync(string topic, string msg);

		private async void OnMessage_Handler(object sender, MqttApplicationMessageReceivedEventArgs e)
		{
			var msg = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
			var topic = e.ApplicationMessage.Topic;

			await this.OnMessageAsync(topic, msg).AwaitBackground();
		}

		private async void OnConnect_Handler(object sender, MqttClientConnectedEventArgs e)
		{
			await this.OnConnectAsync().AwaitBackground();
		}

		private async void OnDisconnect_HandlerAsync(object sender, MqttClientDisconnectedEventArgs e)
		{
			if(this._disconnected)
				return;

			await Task.Delay(TimeSpan.FromSeconds(5D));

			try {
				await this.Client.ConnectAsync(this._client_options);
			} catch {
				this._logger.LogInformation("Unable to reconnect");
			}
		}
	}
}
