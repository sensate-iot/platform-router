/*
 * Match and handle regex trigger actions.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using JetBrains.Annotations;
using MongoDB.Bson;
using Prometheus;

using SensateIoT.Platform.Network.Common.Converters;
using SensateIoT.Platform.Network.Common.Helpers;
using SensateIoT.Platform.Network.Common.MQTT;
using SensateIoT.Platform.Network.Contracts.DTO;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.TriggerService.Abstract;
using SensateIoT.Platform.Network.TriggerService.Config;
using SensateIoT.Platform.Network.TriggerService.DTO;

namespace SensateIoT.Platform.Network.TriggerService.MQTT
{
	[UsedImplicitly]
	public class MqttRegexTriggerHandler : IMqttHandler
	{
		private readonly ILogger<MqttRegexTriggerHandler> m_logger;
		private readonly IRegexMatchingService m_regexMatcher;
		private readonly ITriggerActionCache m_cache;
		private readonly IServiceProvider m_provider;
		private readonly IInternalMqttClient m_client;
		private readonly string m_eventsTopic;

		private readonly Counter m_messageCounter;
		private readonly Counter m_matchCounter;
		private readonly Histogram m_duration;

		public MqttRegexTriggerHandler(
			ILogger<MqttRegexTriggerHandler> logger,
			IRegexMatchingService matcher,
			IServiceProvider provider,
			IInternalMqttClient client,
			IOptions<MqttConfig> options,
			ITriggerActionCache cache
		)
		{
			this.m_logger = logger;
			this.m_regexMatcher = matcher;
			this.m_client = client;
			this.m_eventsTopic = options.Value.InternalBroker.TriggerEventTopic;
			this.m_cache = cache;
			this.m_provider = provider;
			this.m_matchCounter = Metrics.CreateCounter("triggerservice_messages_matched_total", "Total amount of measurements that matched a trigger.");
			this.m_messageCounter = Metrics.CreateCounter("triggerservice_messages_received_total", "Total amount of messages received.");
			this.m_duration = Metrics.CreateHistogram("triggerservice_message_handle_duration_seconds", "Histogram of message handling duration.");
		}

		public async Task OnMessageAsync(string topic, string message, CancellationToken ct = default)
		{
			using(this.m_duration.NewTimer()) {
				await this.HandleMessageAsync(message).ConfigureAwait(false);
			}
		}

		private async Task HandleMessageAsync(string message)
		{
			this.m_logger.LogInformation("Trigger messages received.");

			var sw = Stopwatch.StartNew();
			var messages = this.Decompress(message).ToList();
			this.m_messageCounter.Inc(messages.Count);

			try {
				var results = await Task.WhenAll(messages.Select(this.HandleMeasurement));
				var data = new TriggerEventData();

				foreach(var triggerLists in results) {
					foreach(var triggerEvents in triggerLists) {
						data.Events.AddRange(triggerEvents);
					}
				}

				await this.PublishAsync(data).ConfigureAwait(false);
			} catch(Exception ex) {
				this.m_logger.LogError(ex, "Unable to handle a trigger.");
			}

			sw.Stop();
			this.m_logger.LogInformation("Messages handled. Processing took {duration:c}.", sw.Elapsed);
		}

		private Task<List<TriggerEvent>[]> HandleMeasurement(InternalBulkMessageQueue measurements)
		{
			var actions = this.m_cache.Lookup(measurements.SensorID);
			var tasks = new List<Task<List<TriggerEvent>>>();

			if(actions == null) {
				return Task.FromResult<List<TriggerEvent>[]>(null);
			}

			using var scope = this.m_provider.CreateScope();
			var exec = scope.ServiceProvider.GetRequiredService<ITriggerActionExecutionService>();

			foreach(var m in measurements.Messages) {
				this.m_matchCounter.Inc();
				var matched = this.m_regexMatcher.Match(m, actions).ToList();
				tasks.Add(ExecuteActionsAsync(exec, matched, m));
			}

			return Task.WhenAll(tasks);
		}

		private IEnumerable<InternalBulkMessageQueue> Decompress(string data)
		{
			var bytes = Convert.FromBase64String(data);
			using var to = new MemoryStream();
			using var from = new MemoryStream(bytes);
			using var gzip = new GZipStream(from, CompressionMode.Decompress);

			gzip.CopyTo(to);
			var final = to.ToArray();
			var protoMessages = TextMessageData.Parser.ParseFrom(final);
			var messages =
				from message in protoMessages.Messages
				group message by message.SensorID into g
				select new InternalBulkMessageQueue {
					SensorID = ObjectId.Parse(g.Key),
					Messages = g.Select(MessageProtobufConverter.Convert).ToList()
				};

			this.m_logger.LogInformation("Received {count} messages.", protoMessages.Messages.Count);
			return messages;
		}

		private static async Task<List<TriggerEvent>> ExecuteActionsAsync(ITriggerActionExecutionService exec, IEnumerable<TriggerAction> actions, Message msg)
		{
			var events = new List<TriggerEvent>();
			var tasks = new List<Task>();

			foreach(var action in actions) {
				var result = Replace(action.Message, msg);

				tasks.Add(exec.ExecuteAsync(action, result));
				events.Add(TriggerActionEventConverter.Convert(action));
			}

			await Task.WhenAll(tasks).ConfigureAwait(false);

			return events;
		}

		private static string Replace(string message, Message msg)
		{
			string lon;
			string lat;
			var body = message.Replace("$value", msg.Data.ToString(CultureInfo.InvariantCulture));

			lon = msg.Longitude.ToString(CultureInfo.InvariantCulture);
			lat = msg.Latitude.ToString(CultureInfo.InvariantCulture);

			body = body.Replace("$timestamp", msg.Timestamp.ToString("O"));
			body = body.Replace("$lon", lon);
			body = body.Replace("$lat", lat);

			return body;
		}

		private async Task PublishAsync(IMessage protoEvents)
		{
			await using var measurementStream = new MemoryStream();
			protoEvents.WriteTo(measurementStream);
			var data = measurementStream.ToArray().Compress();
			await this.m_client.PublishOnAsync(this.m_eventsTopic, data, false).ConfigureAwait(false);
		}

	}
}
