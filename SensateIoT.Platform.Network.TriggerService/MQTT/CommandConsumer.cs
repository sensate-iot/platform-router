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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using Newtonsoft.Json;

using SensateIoT.Platform.Network.Common.MQTT;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.Data.Enums;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.TriggerService.Abstract;

namespace SensateIoT.Platform.Network.TriggerService.MQTT
{
	[UsedImplicitly]
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
			this.m_logger.LogInformation("Received command: {command}.", cmd.Cmd);

			using var scope = this.m_provider.CreateScope();
			var cache = scope.ServiceProvider.GetRequiredService<ITriggerActionCache>();

			switch(cmd.Cmd) {
			case CommandType.AddSensor:
				var id = ObjectId.Parse(cmd.Arguments);
				var repo = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();
				var actions = await repo.GetTriggerServiceActionsBySensorId(id, ct)
					.ConfigureAwait(false);

				cache.Load(actions);
				break;

			case CommandType.FlushSensor:
				var sensorId = ObjectId.Parse(cmd.Arguments);
				cache.FlushSensor(sensorId);
				break;
			}
		}
	}
}