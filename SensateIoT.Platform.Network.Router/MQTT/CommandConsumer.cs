/*
 * Consumer to listen to a stream of commands.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using SensateIoT.Platform.Network.Common.Caching.Abstract;
using SensateIoT.Platform.Network.Common.Caching.Routing;
using SensateIoT.Platform.Network.Common.MQTT;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.Data.Enums;

namespace SensateIoT.Platform.Network.Router.MQTT
{
	[UsedImplicitly]
	public class CommandConsumer : IMqttHandler
	{
		private readonly ILogger<CommandConsumer> m_logger;
		private readonly DataUpdateHandler m_handler;
		private readonly LiveDataRouteUpdateHandler m_liveUpdater;
		private readonly CommandCounter m_counter;

		public CommandConsumer(IServiceProvider provider, CommandCounter counter, IRoutingCache cache, ILogger<CommandConsumer> logger)
		{
			this.m_logger = logger;
			this.m_handler = new DataUpdateHandler(cache, provider);
			this.m_liveUpdater = new LiveDataRouteUpdateHandler(cache);
			this.m_counter = counter;
		}

		public async Task OnMessageAsync(string topic, string message, CancellationToken ct = default)
		{
			var cmd = JsonConvert.DeserializeObject<Command>(message);

			this.m_logger.LogInformation("Received command: {command}. Argument: {argument}.", cmd.Cmd, cmd.Arguments);
			this.m_counter.Counter.Inc();

			switch(cmd.Cmd) {
			case CommandType.FlushUser:
			case CommandType.FlushSensor:
			case CommandType.FlushKey:
			case CommandType.AddUser:
			case CommandType.AddSensor:
			case CommandType.AddKey:
			case CommandType.DeleteUser:
				await this.m_handler.UpdateAsync(cmd, ct).ConfigureAwait(false);
				break;

			case CommandType.AddLiveDataSensor:
			case CommandType.RemoveLiveDataSensor:
			case CommandType.SyncLiveDataSensors:
				this.m_liveUpdater.HandleUpdate(cmd);
				break;

			default:
				throw new ArgumentException($"Command not known ({cmd.Cmd:G})", nameof(message));
			}
		}
	}
}
