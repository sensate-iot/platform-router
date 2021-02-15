/*
 * Convert measurements to and from protobuf format.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.TriggerService.Config;

namespace SensateIoT.Platform.Network.TriggerService.Services
{
	public class DataPointMatchingService : IDataPointMatchingService
	{
		private readonly TimeoutConfig m_timeouts;

		public DataPointMatchingService(IOptions<TimeoutConfig> settings)
		{
			this.m_timeouts = settings.Value;
		}

		private TimeSpan GetTimeout(TriggerAction action)
		{
			switch(action.Channel) {
			case TriggerChannel.Email:
				return TimeSpan.FromMinutes(this.m_timeouts.MailTimeout);

			case TriggerChannel.SMS:
				return TimeSpan.FromMinutes(this.m_timeouts.SmsTimeout);

			case TriggerChannel.MQTT:
			case TriggerChannel.ControlMessage:
				return TimeSpan.FromMinutes(this.m_timeouts.ActuatorTimeout);

			case TriggerChannel.HttpPost:
			case TriggerChannel.HttpGet:
				return TimeSpan.FromMinutes(this.m_timeouts.HttpTimeout);

			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public IEnumerable<TriggerAction> Match(string key, DataPoint dp, IList<TriggerAction> actions)
		{
			var list = new List<TriggerAction>();

			foreach(var action in actions) {
				if(action.KeyValue != key) {
					continue;
				}

				var rv = false;

				if(action.LowerEdge != null && action.UpperEdge == null) {
					rv = dp.Value >= action.LowerEdge.Value;
				} else if(action.LowerEdge == null && action.UpperEdge != null) {
					rv = dp.Value <= action.UpperEdge.Value;
				} else if(action.LowerEdge != null && action.UpperEdge != null) {
					rv = dp.Value >= action.LowerEdge.Value && dp.Value <= action.UpperEdge.Value;
				}

				if(!rv) {
					continue;
				}

				/*
				 * Validate timestamps
				 */
				var expiry = action.LastInvocation.Add(this.GetTimeout(action));

				if(expiry > DateTime.UtcNow) {
					continue;
				}

				list.Add(action);
			}

			return list;
		}
	}
}
