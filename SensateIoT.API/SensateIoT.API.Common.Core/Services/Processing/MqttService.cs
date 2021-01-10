/*
 * MQTT background service
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SensateIoT.API.Common.Config.Settings;
using SensateIoT.API.Common.Core.Helpers;

namespace SensateIoT.API.Common.Core.Services.Processing
{
	public class MqttService : AbstractMqttService
	{
		private readonly MqttServiceOptions _options;

		public MqttService(IServiceProvider provider, IOptions<MqttServiceOptions> options, ILogger<MqttService> logger) :
			base(options.Value.Host, options.Value.Port, options.Value.Ssl, options.Value.TopicShare, logger, provider)
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

			this._logger.LogInformation("MQTT client disconnected!");
		}

		protected override async Task OnConnectAsync()
		{
			await base.OnConnectAsync().AwaitBackground();
			this._logger.LogInformation("MQTT client connected!");
		}
	}
}
