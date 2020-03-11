/*
 * MQTT handler for formal languages.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Middleware;
using SensateService.Models;
using SensateService.TriggerHandler.Services;

namespace SensateService.TriggerHandler.Mqtt
{
	public class MqttFormalLanguageTriggerHandler : MqttHandler
	{
		private readonly ILogger<MqttFormalLanguageTriggerHandler> m_logger;
		private readonly ISensorRepository m_sensors;
		private readonly IUserRepository m_users;
		private readonly ITriggerRepository m_triggers;
		private readonly ITriggerHandlerService m_handler;

		public MqttFormalLanguageTriggerHandler(
			ILogger<MqttFormalLanguageTriggerHandler> logger,
			ISensorRepository sensors,
			IUserRepository users,
			ITriggerRepository triggers,
			ITriggerHandlerService handler
		)
		{
			this.m_logger = logger;
			this.m_users = users;
			this.m_sensors = sensors;
			this.m_triggers = triggers;
			this.m_handler = handler;
		}

		public override void OnMessage(string topic, string msg)
		{
			Task.Run(async () => { await this.OnMessageAsync(topic, msg).AwaitBackground(); }).Wait();
		}

		public override async Task OnMessageAsync(string topic, string message)
		{
			try {
				var msg = JsonConvert.DeserializeObject<Message>(message);
				var triggers_enum = await this.m_triggers.GetAsync(msg.SensorId.ToString(), TriggerType.Regex).AwaitBackground();
				var triggers = triggers_enum.ToList();
				var tasks = new List<Task>();

				if(triggers.Count <= 0) {
					return;
				}

				var sensor = await this.m_sensors.GetAsync(msg.SensorId.ToString()).AwaitBackground();
				var user = await this.m_users.GetAsync(sensor.Owner).AwaitBackground();

				foreach(var trigger in triggers) {
					var regex = new Regex(trigger.FormalLanguage);

					if(!regex.IsMatch(msg.Data)) {
						return;
					}

					var last = trigger.Invocations.OrderByDescending(x => x.Timestamp).FirstOrDefault();

					foreach(var action in trigger.Actions) {
						tasks.Add(this.m_handler.HandleTriggerAction(user, trigger, action, last, action.Message));
					}
				}

				await Task.WhenAll(tasks).AwaitBackground();
			} catch(Exception ex) {
				this.m_logger.LogInformation($"Unable to handle message trigger: {ex.Message}");
				this.m_logger.LogDebug(ex.StackTrace);
			}
		}
	}
}
