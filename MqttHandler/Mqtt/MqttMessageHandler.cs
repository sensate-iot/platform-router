/*
 * MQTT handler for incoming messages.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MongoDB.Bson;
using Newtonsoft.Json;

using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.In;
using SensateService.Services;
using SensateService.Services.Settings;

namespace SensateService.MqttHandler.Mqtt
{
	public class MqttMessageHandler : Middleware.MqttHandler
	{
		private readonly InternalMqttServiceOptions m_options;
		private IUserRepository m_users;
		private IMessageRepository m_messages;
		private ISensorRepository m_sensors;
		private readonly IMqttPublishService m_client;
		private readonly ILogger<MqttMessageHandler> m_logger;

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

		public override async Task OnMessageAsync(string topic, string message)
		{
			try {
				var raw = JsonConvert.DeserializeObject<RawMessage>(message);

				if(!ObjectId.TryParse(raw.SensorId, out var id)) {
					this.m_logger.LogDebug("Unable to parse a raw message's sensor ID.");
					return;
				}

				var sensor = await this.m_sensors.GetAsync(raw.SensorId).AwaitBackground();
				var user = await this.m_users.GetAsync(sensor.Owner).AwaitBackground();
				var key = user.ApiKeys.FirstOrDefault(x => x.ApiKey == sensor.Secret);
				var asyncio = new Task[2];

				if(user.UserRoles.Any(x => x.Role.Name == SensateRole.Banned)) {
					this.m_logger.LogDebug("Banned user attempted to store a message.");
					return;
				}

				if(key == null || key.Revoked || key.Type != ApiKeyType.SensorKey) {
					this.m_logger.LogDebug("Attempted to store message using an invalid key.");
					return;
				}

				var msg = new Message {
					Data = raw.Data,
					SensorId = id,
					CreatedAt = raw.CreatedAt ?? DateTime.UtcNow,
					UpdatedAt = raw.CreatedAt ?? DateTime.UtcNow
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
