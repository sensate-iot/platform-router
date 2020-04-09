/*
 * Websocket handler used to receive messages.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SensateService.Enums;
using SensateService.Exceptions;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Generic;
using SensateService.Models.Json.In;
using SensateService.Services;
using SensateService.Services.Settings;

namespace SensateService.WebSocketHandler.Handlers
{
	public class WebSocketMessageHandler : Middleware.WebSocketHandler
	{
		private readonly IMqttPublishService m_client;
		private readonly InternalMqttServiceOptions m_options;
		private readonly IServiceProvider provider;
		private readonly ILogger<WebSocketMessageHandler> m_logger;

		public WebSocketMessageHandler(IMqttPublishService client, IServiceProvider provider,
			IOptions<InternalMqttServiceOptions> options, ILogger<WebSocketMessageHandler> logger)
		{
			this.provider = provider;
			this.m_client = client;
			this.m_options = options.Value;
			this.m_logger = logger;
		}

		public override async Task Receive(AuthenticatedWebSocket socket, WebSocketReceiveResult result, byte[] buffer)
		{
			string raw_msg;
			RawMessage raw;

			raw_msg = Encoding.UTF8.GetString(buffer, 0, result.Count);

			try {
				raw = JsonConvert.DeserializeObject<RawMessage>(raw_msg);

				using var scope = this.provider.CreateScope();
				var sensorsdb = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
				var messagedb = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
				var usersdb = scope.ServiceProvider.GetRequiredService<IUserRepository>();

				if(!ObjectId.TryParse(raw.SensorId, out var id)) {
					this.m_logger.LogDebug("Unable to parse a raw message's sensor ID.");
					return;
				}

				var sensor = await sensorsdb.GetAsync(raw.SensorId).AwaitBackground();
				var user = await usersdb.GetAsync(sensor.Owner).AwaitBackground();
				var key = user.ApiKeys.FirstOrDefault(x => x.ApiKey == sensor.Secret);
				var asyncio = new Task[2];

				if(user.BillingLockout) {
					return;
				}

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

				asyncio[0] = messagedb.CreateAsync(msg);
				asyncio[1] = this.m_client.PublishOnAsync(this.m_options.InternalMessageTopic, json, false);
				await Task.WhenAll(asyncio).AwaitBackground();

				this.m_logger.LogInformation("Message stored!");
			} catch(InvalidRequestException ex) {
				Debug.WriteLine($"Unable to store message: {ex.Message}");
				dynamic jobj = new JObject();
				jobj.status = ex.ErrorCode;
				await this.SendMessage(socket.Raw, jobj.ToString());
			} catch(Exception ex) {
				Debug.WriteLine($"Unable to store message: {ex.Message}");
				dynamic jobj = new JObject();
				jobj.status = 500;
				await this.SendMessage(socket.Raw, jobj.ToString());
			}
		}
	}
}