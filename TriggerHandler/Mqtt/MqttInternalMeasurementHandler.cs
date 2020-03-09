/*
 * MQTT measurement handler.
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
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MongoDB.Bson;

using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Generic;
using SensateService.Services;
using SensateService.Services.Settings;
using SensateService.TriggerHandler.Application;
using SensateService.TriggerHandler.Models;

using Convert = System.Convert;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace SensateService.TriggerHandler.Mqtt
{
	public class MqttInternalMeasurementHandler : Middleware.MqttHandler
	{
		private readonly IServiceProvider m_provider;
		private readonly ILogger<MqttInternalMeasurementHandler> logger;
		private readonly TextServiceSettings m_textSettings;
		private readonly TimeoutSettings m_timeoutSettings;
		private readonly MqttPublishServiceOptions m_options;

		public MqttInternalMeasurementHandler(IServiceProvider provider,
			IOptions<TextServiceSettings> text_opts,
			IOptions<MqttPublishServiceOptions> options,
			IOptions<TimeoutSettings> timeout,
			ILogger<MqttInternalMeasurementHandler> logger)
		{
			this.m_provider = provider;
			this.logger = logger;
			this.m_textSettings = text_opts.Value;
			this.m_timeoutSettings = timeout.Value;
			this.m_options = options.Value;
		}

		public override void OnMessage(string topic, string msg)
		{
			Task.Run(async () => { await this.OnMessageAsync(topic, msg).AwaitBackground(); }).Wait();
		}

		public static string Decompress(string data)
		{
			var decoded = Convert.FromBase64String(data);
			MemoryStream msi, mso;
			GZipStream gs;
			string rv;

			msi = mso = null;

			try {
				msi = new MemoryStream(decoded);
				mso = new MemoryStream();

				gs = new GZipStream(msi, CompressionMode.Decompress);
				gs.CopyTo(mso);
				gs.Dispose();

				rv = Encoding.UTF8.GetString(mso.ToArray());
			} finally {
				mso?.Dispose();
				msi?.Dispose();
			}

			return rv;
		}

		private static bool MatchDatapoint(Trigger trigger, DataPoint dp)
		{
			bool rv;

			rv = false;

			if(trigger.LowerEdge != null && trigger.UpperEdge == null) {
				rv = dp.Value >= trigger.LowerEdge.Value;
			} else if(trigger.LowerEdge == null && trigger.UpperEdge != null) {
				rv = dp.Value <= trigger.UpperEdge.Value;
			} else if(trigger.LowerEdge != null && trigger.UpperEdge != null) {
				rv = dp.Value >= trigger.LowerEdge.Value && dp.Value <= trigger.UpperEdge.Value;
			}

			return rv;
		}

		private static bool CanExecute(TriggerInvocation last, int timeout)
		{
			if(last == null) {
				return true;
			}

			var nextAvailable = last.Timestamp.AddMinutes(timeout);
			var rv = nextAvailable.DateTime.ToUniversalTime() < DateTime.UtcNow;

			return rv;
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

		private async Task HandleTriggers(IUserRepository usersdb, ISensorRepository sensorsdb,
			IControlMessageRepository controldb, IList<Tuple<Trigger, TriggerInvocation, DataPoint>> invocations,
			IServiceProvider provider)
		{
			var distinctSensors = invocations.Select(x => x.Item1.SensorId).Distinct();
			var enum_sensors = await sensorsdb.GetAsync(distinctSensors).AwaitBackground();
			var sensors = enum_sensors.ToList();
			var users = await usersdb.GetRangeAsync(sensors.Select(x => x.Owner).Distinct()).AwaitBackground();

			var smsService = provider.GetRequiredService<ITextSendService>();
			var emailService = provider.GetRequiredService<IEmailSender>();
			var publishService = provider.GetRequiredService<IMqttPublishService>();

			var usersMap = users.ToDictionary(x => x.Id, x => x);
			var sensorsMap = sensors.ToDictionary(x => x.InternalId.ToString(), x => x);
			var tasks = new List<Task>();
			var client = new RestClient();

			foreach(var (trigger, _, dp) in invocations) {
				var sensor = sensorsMap[trigger.SensorId];
				var user = usersMap[sensor.Owner];
				var last = trigger.Invocations.OrderByDescending(x => x.Timestamp).FirstOrDefault();

				foreach(var action in trigger.Actions) {
					var body = Replace(action, dp);

					switch(action.Channel) {
						case TriggerActionChannel.Email:
							if(!user.EmailConfirmed) {
								continue;
							}

							if(CanExecute(last, this.m_timeoutSettings.MailTimeout)) {
								var mail = new EmailBody {
									HtmlBody = body,
									TextBody = body
								};

								tasks.Add(emailService.SendEmailAsync(user.Email, "Sensate trigger triggered", mail));
							}

							break;
						case TriggerActionChannel.SMS:
							if(CanExecute(last, this.m_timeoutSettings.MessageTimeout)) {
								if(!user.PhoneNumberConfirmed)
									continue;

								tasks.Add(smsService.SendAsync(this.m_textSettings.AlphaCode, user.PhoneNumber, body));
							}

							break;

						case TriggerActionChannel.MQTT:
							if(CanExecute(last, this.m_timeoutSettings.MqttTimeout)) {
								var topic = $"sensate/trigger/{trigger.SensorId}";
								tasks.Add(publishService.PublishOnAsync(topic, body, false));
							}
							break;

						case TriggerActionChannel.HttpPost:
						case TriggerActionChannel.HttpGet:
							var result = Uri.TryCreate(action.Target, UriKind.Absolute, out var output) &&
										  output.Scheme == Uri.UriSchemeHttp || output.Scheme == Uri.UriSchemeHttps;

							if(!result) {
								break;
							}

							if(!CanExecute(last, this.m_timeoutSettings.HttpTimeout)) {
								break;
							}

							tasks.Add(action.Channel == TriggerActionChannel.HttpGet
								? client.GetAsync(action.Target)
								: client.PostAsync(action.Target, body));
							break;

						case TriggerActionChannel.ControlMessage:
							if(!ObjectId.TryParse(action.Target, out var id)) {
								break;
							}

							var msg = new ControlMessage {
								Data = body,
								SensorId = id,
								Timestamp = DateTime.UtcNow
							};

							var actuator = this.m_options.ActuatorTopic.Replace("$sensorId", action.Target);

							var io = new[] {
								publishService.PublishOnAsync(actuator, body, false),
								controldb.CreateAsync(msg)
							};

							await Task.WhenAll(io).AwaitBackground();
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}

			await Task.WhenAll(tasks).AwaitBackground();
		}

		public override async Task OnMessageAsync(string topic, string message)
		{
			this.logger.LogDebug("Message received!");

			var data = Decompress(message);
			var measurements = JsonConvert.DeserializeObject<IList<InternalMeasurement>>(data);

			using var scope = this.m_provider.CreateScope();
			var triggersdb = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();
			var usersdb = scope.ServiceProvider.GetRequiredService<IUserRepository>();
			var sensorsdb = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
			var controldb = scope.ServiceProvider.GetRequiredService<IControlMessageRepository>();

			var ids = measurements.Select(m => m.CreatedBy).Distinct().ToList();

			var raw_triggers = await triggersdb.GetAsync(ids).AwaitBackground();
			var triggers = raw_triggers.ToList();
			var triggered = new List<Tuple<Trigger, TriggerInvocation, DataPoint>>();

			foreach(var mCollection in measurements) {
				var subset = triggers.Where(trigger => trigger.SensorId == mCollection.CreatedBy).ToList();

				if(subset.Count <= 0)
					continue;

				foreach(var measurement in mCollection.Measurements) {
					foreach(var t in subset) {
						if(!measurement.Data.TryGetValue(t.KeyValue, out var datapoint)) {
							continue;
						}

						if(!MatchDatapoint(t, datapoint)) {
							continue;
						}

						var inv = new TriggerInvocation {
							TriggerId = t.Id,
							Timestamp = new DateTimeOffset(measurement.Timestamp.ToUniversalTime(), TimeSpan.Zero),
						};


						triggered.Add(new Tuple<Trigger, TriggerInvocation, DataPoint>(t, inv, datapoint));
					}
				}
			}

			var distinct = triggered.GroupBy(t => t.Item2.TriggerId)
				.Select(g => g.First()).ToList();

			await HandleTriggers(usersdb, sensorsdb, controldb, distinct, scope.ServiceProvider).AwaitBackground();
			await triggersdb.AddInvocationsAsync(distinct.Select(t => t.Item2)).AwaitBackground();

			this.logger.LogDebug($"{triggered.Count} triggers triggered!");
			this.logger.LogDebug($"{distinct.Count} triggers handled!");
		}
	}
}

