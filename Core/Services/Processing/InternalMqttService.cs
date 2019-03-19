/*
 * Internal MQTT publish service.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MQTTnet;

using SensateService.Helpers;
using SensateService.Services.Settings;

namespace SensateService.Services.Processing
{
	public class InternalMqttService : AbstractMqttService, IMqttPublishService
	{
		private readonly InternalMqttServiceOptions _options;

		public InternalMqttService(IOptions<InternalMqttServiceOptions> options, ILogger<MqttService> logger) :
			base(options.Value.Host, options.Value.Port, options.Value.Ssl, logger)
		{
			this._options = options.Value;
		}

		public override async Task ExecuteAsync(CancellationToken token)
		{
			await this.Connect(this._options.Username, this._options.Password).AwaitBackground();
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			await base.StopAsync(cancellationToken);
			await this.Disconnect().AwaitBackground();

			this._logger.LogInformation("Internal MQTT client disconnected!");
		}

		public async Task PublishOnAsync(string topic, string message, bool retain)
		{
			MqttApplicationMessageBuilder builder;
			MqttApplicationMessage msg;

			if(!this.Client.IsConnected)
				return;

			builder = new MqttApplicationMessageBuilder();
			builder.WithAtLeastOnceQoS();
			builder.WithPayload(message);
			builder.WithTopic(topic);
			builder.WithRetainFlag(retain);
			msg = builder.Build();
			await this.Client.PublishAsync(msg);
		}

		public void PublishOn(string topic, string message, bool retain)
		{
			var task = Task.Run(async () => { await this.PublishOnAsync(topic, message, retain).AwaitBackground(); });
			task.Wait();
		}

		protected override Task OnConnectAsync()
		{
			this._logger.LogInformation("Internal MQTT client connected!");
			return Task.CompletedTask;
		}

		protected override Task OnMessageAsync(string topic, string msg)
		{
			return Task.CompletedTask;
		}
	}
}
