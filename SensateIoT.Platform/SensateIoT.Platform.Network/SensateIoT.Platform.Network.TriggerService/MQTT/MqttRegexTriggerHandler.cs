/*
 * Match and handle regex trigger actions.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using Prometheus;

using SensateIoT.Platform.Network.Common.Converters;
using SensateIoT.Platform.Network.Common.MQTT;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.TriggerService.DTO;
using SensateIoT.Platform.Network.TriggerService.Services;

namespace SensateIoT.Platform.Network.TriggerService.MQTT
{
	public class MqttRegexTriggerHandler : IMqttHandler
	{
		private readonly ILogger<MqttRegexTriggerHandler> m_logger;
		private readonly ITriggerRepository m_repo;
		private readonly IRegexMatchingService m_regexMatcher;
		private readonly ITriggerActionExecutionService m_exec;
		private readonly Counter m_messageCounter;
		private readonly Counter m_matchCounter;

		public MqttRegexTriggerHandler(
			ILogger<MqttRegexTriggerHandler> logger,
			ITriggerRepository triggers,
			IRegexMatchingService matcher,
			ITriggerActionExecutionService exec
		)
		{
			this.m_logger = logger;
			this.m_repo = triggers;
			this.m_regexMatcher = matcher;
			this.m_exec = exec;
			this.m_matchCounter = Metrics.CreateCounter("triggerservice_messages_matched_total", "Total amount of measurements that matched a trigger.");
			this.m_messageCounter = Metrics.CreateCounter("triggerservice_messages_received_total", "Total amount of messages received.");
		}

		private static IEnumerable<InternalBulkMessageQueue> Decompress(string data)
		{
			var bytes = Convert.FromBase64String(data);
			using var to = new MemoryStream();
			using var from = new MemoryStream(bytes);
			using var gzip = new GZipStream(@from, CompressionMode.Decompress);

			gzip.CopyTo(to);
			var final = to.ToArray();
			var protoMeasurements = TextMessageData.Parser.ParseFrom(final);
			var messages =
				from message in protoMeasurements.Messages
				group message by message.SensorID into g
				select new InternalBulkMessageQueue {
					SensorID = ObjectId.Parse(g.Key),
					Messages = g.Select(MessageProtobufConverter.Convert).ToList()
				};

			return messages;
		}

		public async Task OnMessageAsync(string topic, string message, CancellationToken ct = default)
		{
			this.m_logger.LogDebug("Trigger messages received.");
			var tasks = new List<Task>();

			var messages = Decompress(message).ToList();
			var triggers = await this.m_repo.GetTriggerServiceActions(messages.Select(x => x.SensorID), ct).ConfigureAwait(false);
			var triggerMap = triggers
				.GroupBy(x => x.SensorID, x => x)
				.ToDictionary(x => x.Key, x => x.ToList());

			this.m_messageCounter.Inc(messages.Count);

			foreach(var metaMessages in messages) {
				var actions = triggerMap[metaMessages.SensorID];

				if(actions == null) {
					continue;
				}

				foreach(var m in metaMessages.Messages) {
					this.m_matchCounter.Inc();

					var matched = this.m_regexMatcher.Match(m, actions);
					tasks.Add(this.ExecuteActionsAsync(matched, m));
				}
			}

			await Task.WhenAll(tasks);

			this.m_logger.LogDebug("Messages handled.");
		}

		private Task ExecuteActionsAsync(IEnumerable<TriggerAction> actions, Message msg)
		{
			var tasks = new List<Task>();

			foreach(var action in actions) {
				var result = Replace(action.Message, msg);
				tasks.Add(this.m_exec.ExecuteAsync(action, result));
			}

			return Task.WhenAll(tasks);
		}

		private static string Replace(string message, Message msg)
		{
			string lon;
			string lat;
			var body = message.Replace("$value", msg.Data.ToString(CultureInfo.InvariantCulture));

			lon = msg.Longitude.ToString(CultureInfo.InvariantCulture);
			lat = msg.Latitude.ToString(CultureInfo.InvariantCulture);

			body = body.Replace("$lon", lon);
			body = body.Replace("$lat", lat);

			return body;
		}
	}
}
