/*
 * Authorization command (MQTT) handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SensateService.Common.Data.Dto.Generic;
using SensateService.Common.Data.Enums;
using SensateService.Constants;
using SensateService.Infrastructure.Authorization;
using SensateService.Middleware;

namespace SensateService.Processing.DataAuthorizationApi.EventHandlers
{
	public class CommandSubscription : MqttHandler
	{
		private readonly ILogger<CommandSubscription> m_logger;
		private readonly IAuthorizationCache m_cache;

		public CommandSubscription(ILogger<CommandSubscription> logger, IAuthorizationCache cache)
		{
			this.m_logger = logger;
			this.m_cache = cache;
		}

		public override void OnMessage(string topic, string msg)
		{
			throw new ApplicationException("Synchronous message handler not implemented!");
		}

		public override Task OnMessageAsync(string topic, string message)
		{
			try {
				var token = JToken.Parse(message);
				var cmd = new Command {
					Cmd = ParseCommand(token[Commands.CommandKey]?.ToString()),
					Arguments = token[Commands.ArgumentKey]?.ToString()
				};

				this.m_cache.AddCommand(cmd);
			} catch(JsonSerializationException ex) {
				this.m_logger.LogError(ex, "Unable to parse command: {message}", ex.Message);
			} catch(FormatException ex) {
				this.m_logger.LogError(ex, "Invalid command received! ({cmd})", ex.Message);
			}

			return Task.CompletedTask;
		}

		private static AuthServiceCommand ParseCommand(string cmd)
		{
			if(cmd == null) {
				throw new ArgumentNullException(nameof(cmd));
			}

			return cmd switch {
				Commands.FlushKey => AuthServiceCommand.FlushKey,
				Commands.FlushSensor => AuthServiceCommand.FlushSensor,
				Commands.FlushUser => AuthServiceCommand.FlushUser,
				Commands.AddUser => AuthServiceCommand.AddUser,
				Commands.AddSensor => AuthServiceCommand.AddSensor,
				Commands.AddKey => AuthServiceCommand.AddKey,
				_ => throw new FormatException(cmd)
			};
		}
	}
}