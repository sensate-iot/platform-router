/*
 * Trigger handling service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.TriggerHandler.Services
{
	public class TriggerTextMatchingService : ITriggerTextMatchingService
	{
		private readonly ISensorRepository m_sensors;
		private readonly IUserRepository m_users;
		private readonly ITriggerHandlerService m_handler;

		public TriggerTextMatchingService(
			ISensorRepository sensors,
			IUserRepository users,
			ITriggerHandlerService handler
		)
		{
			this.m_sensors = sensors;
			this.m_users = users;
			this.m_handler = handler;
		}

		public async Task HandleTriggerAsync(IList<Tuple<Trigger, TriggerInvocation>> invocations)
		{
			var distinctSensors = invocations.Select(x => x.Item1.SensorId).Distinct();
			var enum_sensors = await this.m_sensors.GetAsync(distinctSensors).AwaitBackground();
			var sensors = enum_sensors.ToList();
			var users = await this.m_users.GetRangeAsync(sensors.Select(x => x.Owner).Distinct()).AwaitBackground();

			var usersMap = users.ToDictionary(x => x.Id, x => x);
			var sensorsMap = sensors.ToDictionary(x => x.InternalId.ToString(), x => x);
			var tasks = new List<Task>();

			foreach(var (trigger, _) in invocations) {
				var sensor = sensorsMap[trigger.SensorId];
				var user = usersMap[sensor.Owner];
				var last = trigger.Invocations.OrderByDescending(x => x.Timestamp).FirstOrDefault();

				tasks.AddRange(
					from action in trigger.Actions
					let body = action.Message
					select this.m_handler.HandleTriggerAction(user, trigger, action, last, body)
				);
			}

			await Task.WhenAll(tasks).AwaitBackground();
		}
	}
}
