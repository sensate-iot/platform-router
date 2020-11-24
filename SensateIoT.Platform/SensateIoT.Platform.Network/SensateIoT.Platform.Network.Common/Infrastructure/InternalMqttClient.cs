/*
 * Internal MQTT publish service.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MQTTnet;
using MQTTnet.Client;
using SensateIoT.Platform.Network.Common.Settings;

namespace SensateIoT.Platform.Network.Common.Infrastructure
{
	public class InternalMqttService : AbstractMqttClient 
	{
		private readonly MqttServiceOptions _options;

		public InternalMqttService(IOptions<MqttServiceOptions> options, ILogger<MqttClient> logger, IServiceProvider sp) :
			base(options.Value.Host, options.Value.Port, options.Value.Ssl, options.Value.TopicShare, logger, sp)
		{
			this._options = options.Value;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			await this.Connect(this._options.Username, this._options.Password).ConfigureAwait(false);
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			await base.StopAsync(cancellationToken);
			await this.Disconnect().ConfigureAwait(false);

			this._logger.LogInformation("Internal MQTT client disconnected!");
		}

		public async Task PublishOnAsync(string topic, string message, bool retain)
		{
			MqttApplicationMessageBuilder builder;
			MqttApplicationMessage msg;

			if (!this.Client.IsConnected)
				return;

			builder = new MqttApplicationMessageBuilder();
			builder.WithAtLeastOnceQoS();
			builder.WithPayload(message);
			builder.WithTopic(topic);
			builder.WithRetainFlag(retain);
			msg = builder.Build();

			await this.Client.PublishAsync(msg).ConfigureAwait(false);
		}

		public void PublishOn(string topic, string message, bool retain)
		{
			var task = Task.Run(async () => { await this.PublishOnAsync(topic, message, retain).ConfigureAwait(false); });
			task.Wait();
		}

		protected override async Task OnConnectAsync()
		{
			await base.OnConnectAsync().ConfigureAwait(false);
			this._logger.LogInformation("Internal MQTT client connected!");
		}
	}
}
