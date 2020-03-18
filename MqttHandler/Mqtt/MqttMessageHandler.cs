/*
 * MQTT handler for incoming messages.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.MqttHandler.Models;
using SensateService.Services;
using SensateService.Services.Settings;

namespace SensateService.MqttHandler.Mqtt
{
	public class MqttMessageHandler : Middleware.MqttHandler
	{
		private readonly InternalMqttServiceOptions m_options;
		private readonly IUserRepository m_users;
		private readonly IMessageRepository m_messages;
		private readonly ISensorRepository m_sensors;
		private readonly IMqttPublishService m_client;
		private readonly ILogger<MqttMessageHandler> m_logger;

		private const int SecretSubStringOffset = 3;
		private const int SecretSubStringStart = 1;

		public MqttMessageHandler(IMessageRepository messages, ISensorRepository sensors, IUserRepository users,
			IOptions<InternalMqttServiceOptions> options, IMqttPublishService client, ILogger<MqttMessageHandler> logger)
		{
			this.m_messages = messages;
			this.m_users = users;
			this.m_sensors = sensors;
			this.m_options = options.Value;
			this.m_client = client;
			this.m_logger = logger;
		}

		public override void OnMessage(string topic, string msg)
		{
		}

		private static byte[] HexToByteArray(string hex)
		{
			var NumberChars = hex.Length;
			var bytes = new byte[NumberChars / 2];

			for(var i = 0; i < NumberChars; i += 2) {
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			}

			return bytes;
		}

		private static bool CompareHashes(ReadOnlySpan<byte> h1, ReadOnlySpan<byte> h2)
		{
			return h1.SequenceEqual(h2);
		}

		public override async Task OnMessageAsync(string topic, string message)
		{
			try {
				var raw = JsonConvert.DeserializeObject<RawMessage>(message);

				var sensor = await this.m_sensors.GetAsync(raw.SensorId.ToString()).AwaitBackground();
				var user = await this.m_users.GetAsync(sensor.Owner).AwaitBackground();
				var asyncio = new Task[2];

				if(user.UserRoles.Any(x => x.Role.Name == SensateRole.Banned)) {
					this.m_logger.LogDebug("Banned user attempted to store a message.");
					return;
				}

				using var sha = SHA256.Create();

				var withSecret = message.Replace(raw.Secret, sensor.Secret);

				var length = raw.Secret.Length - SecretSubStringOffset;
				var binary = Encoding.ASCII.GetBytes(withSecret);
				var computed = sha.ComputeHash(binary);
				var hash = HexToByteArray(raw.Secret.Substring(SecretSubStringStart, length));

				if(!CompareHashes(computed, hash)) {
					return;
				}

				var msg = new Message {
					Data = raw.Data,
					SensorId = raw.SensorId,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				var json = JsonConvert.SerializeObject(msg);

				asyncio[0] = this.m_messages.CreateAsync(msg);
				asyncio[1] = this.m_client.PublishOnAsync(this.m_options.InternalMessageTopic, json, false);
				await Task.WhenAll(asyncio).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to store message: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);

				return;
			}

			this.m_logger.LogInformation("Message stored!");
		}
	}
}
