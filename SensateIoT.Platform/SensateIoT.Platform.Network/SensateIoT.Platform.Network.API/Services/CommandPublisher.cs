/*
 * Command publishing service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SensateIoT.Platform.Network.API.Abstract;
using SensateIoT.Platform.Network.API.Config;
using SensateIoT.Platform.Network.Common.Constants;
using SensateIoT.Platform.Network.Common.MQTT;
using SensateIoT.Platform.Network.Data.Enums;

namespace SensateIoT.Platform.Network.API.Services
{
	public class CommandPublisher : ICommandPublisher
	{
		private readonly IInternalMqttClient m_mqtt;
		private readonly string m_topic;

		public CommandPublisher(IOptions<InternalBrokerConfig> options, IInternalMqttClient mqtt)
		{
			this.m_mqtt = mqtt;
			this.m_topic = options.Value.CommandTopic;
		}

		public async Task PublishCommandAsync(CommandType type, string argument, CancellationToken ct = default)
		{
			var obj = new JObject {
				[Commands.CommandKey] = type switch
				{
					CommandType.FlushKey => Commands.FlushKey,
					CommandType.FlushSensor => Commands.FlushSensor,
					CommandType.FlushUser => Commands.FlushUser,
					CommandType.DeleteUser => Commands.DeleteUser,
					CommandType.AddUser => Commands.AddUser,
					CommandType.AddSensor => Commands.AddSensor,
					CommandType.AddKey => Commands.AddKey,
					_ => throw new ArgumentOutOfRangeException(nameof(type))
				},
				[Commands.ArgumentKey] = argument
			};

			await this.m_mqtt.PublishOnAsync(this.m_topic, obj.ToString(Formatting.None), false).ConfigureAwait(false);
		}
	}
}
