/*
 * Consumer to listen to a stream of commands.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using SensateIoT.Platform.Network.API.Abstract;
using SensateIoT.Platform.Network.Common.MQTT;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.Data.Enums;

namespace SensateIoT.Platform.Network.API.MQTT
{
	public class CommandConsumer : IMqttHandler
	{
		private readonly ILogger<CommandConsumer> m_logger;
		private readonly IServiceProvider m_provider;

		public CommandConsumer(IServiceProvider provider, ILogger<CommandConsumer> logger)
		{
			this.m_logger = logger;
			this.m_provider = provider;
		}

		public async Task OnMessageAsync(string topic, string message, CancellationToken ct = default)
		{
			var cmd = JsonConvert.DeserializeObject<Command>(message);
			this.m_logger.LogInformation("Received command: {command}. Argument: {argument}.", cmd.Cmd, cmd.Arguments);

			if(cmd.Cmd == CommandType.DeleteUser) {
				using var scope = this.m_provider.CreateScope();
				var repo = scope.ServiceProvider.GetRequiredService<ISensorService>();
				var userId = Guid.Parse(cmd.Arguments);

				await repo.DeleteAsync(userId, ct).ConfigureAwait(false);
				this.m_logger.LogInformation("Flushed user with ID: {userId}.", cmd.Arguments);
			}
		}
	}
}
