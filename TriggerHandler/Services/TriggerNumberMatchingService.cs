/*
 * Trigger handling service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using SensateService.Common.Data.Models;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;

namespace SensateService.TriggerHandler.Services
{
	public class TriggerNumberMatchingService : ITriggerNumberMatchingService
	{
		private readonly ISensorRepository m_sensors;
		private readonly IUserRepository m_users;
		private readonly ITriggerHandlerService m_handler;

		public TriggerNumberMatchingService(
			ISensorRepository sensors,
			IUserRepository users,
			ITriggerHandlerService handler
		)
		{
			this.m_sensors = sensors;
			this.m_users = users;
			this.m_handler = handler;
		}

		private static string Replace(TriggerAction action, DataPoint dp)
		{
			string precision;
			string accuracy;
			var body = action.Message.Replace("$value", dp.Value.ToString(CultureInfo.InvariantCulture));

			body = body.Replace("$unit", dp.Unit);

			precision = dp.Precision != null ? dp.Precision.Value.ToString(CultureInfo.InvariantCulture) : "";
			accuracy = dp.Accuracy != null ? dp.Accuracy.Value.ToString(CultureInfo.InvariantCulture) : "";

			body = body.Replace("$precision", precision);
			body = body.Replace("$accuracy", accuracy);

			return body;
		}


		public async Task HandleTriggerAsync(IList<Tuple<Trigger, TriggerInvocation, DataPoint>> invocations)
		{
			var distinctSensors = invocations.Select(x => x.Item1.SensorId).Distinct();
			var enum_sensors = await this.m_sensors.GetAsync(distinctSensors).AwaitBackground();
			var sensors = enum_sensors.ToList();
			var users = await this.m_users.GetRangeAsync(sensors.Select(x => x.Owner).Distinct()).AwaitBackground();

			var usersMap = users.ToDictionary(x => x.Id, x => x);
			var sensorsMap = sensors.ToDictionary(x => x.InternalId.ToString(), x => x);
			var tasks = new List<Task>();

			foreach(var (trigger, _, dp) in invocations) {
				var sensor = sensorsMap[trigger.SensorId];
				var user = usersMap[sensor.Owner];
				var last = trigger.Invocations.OrderByDescending(x => x.Timestamp).FirstOrDefault();

				foreach(var action in trigger.Actions) {
					var body = Replace(action, dp);
					tasks.Add(this.m_handler.HandleTriggerAction(user, trigger, action, last, body));
				}
			}

			await Task.WhenAll(tasks).AwaitBackground();
		}
	}
}
