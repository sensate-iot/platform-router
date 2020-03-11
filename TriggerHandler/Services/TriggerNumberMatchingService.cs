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

namespace SensateService.TriggerHandler.Services
{
	public class TriggerNumberMatchingService : ITriggerNumberMatchingService
	{
		private readonly ISensorRepository m_sensors;
		private readonly IControlMessageRepository m_conrol;
		private readonly ITextSendService m_text;
		private readonly IUserRepository m_users;
		private readonly IEmailSender m_mail;
		private readonly IMqttPublishService m_publisher;
		private readonly TimeoutSettings m_timeout;
		private readonly MqttPublishServiceOptions m_mqttSettings;
		private readonly TextServiceSettings m_textSettings;

		public TriggerNumberMatchingService(
			ISensorRepository sensors,
			ITriggerRepository trigger,
			IControlMessageRepository conrol,
			ITextSendService text,
			IUserRepository users,
			IEmailSender mail,
			IMqttPublishService publisher,
			IOptions<TextServiceSettings> text_opts,
			IOptions<MqttPublishServiceOptions> options,
			IOptions<TimeoutSettings> timeout
		)
		{
			this.m_sensors = sensors;
			this.m_conrol = conrol;
			this.m_text = text;
			this.m_users = users;
			this.m_mail = mail;
			this.m_publisher = publisher;
			this.m_mqttSettings = options.Value;
			this.m_timeout = timeout.Value;
			this.m_textSettings = text_opts.Value;
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

		private static bool CanExecute(TriggerInvocation last, int timeout)
		{
			if(last == null) {
				return true;
			}

			var nextAvailable = last.Timestamp.AddMinutes(timeout);
			var rv = nextAvailable.DateTime.ToUniversalTime() < DateTime.UtcNow;

			return rv;
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

						if(CanExecute(last, this.m_timeout.MailTimeout)) {
							var mail = new EmailBody {
								HtmlBody = body,
								TextBody = body
							};

							tasks.Add(this.m_mail.SendEmailAsync(user.Email, "Sensate trigger triggered", mail));
						}

						break;
					case TriggerActionChannel.SMS:
						if(CanExecute(last, this.m_timeout.MessageTimeout)) {
							if(!user.PhoneNumberConfirmed)
								continue;

							tasks.Add(this.m_text.SendAsync(this.m_textSettings.AlphaCode, user.PhoneNumber, body));
						}

						break;

					case TriggerActionChannel.MQTT:
						if(CanExecute(last, this.m_timeout.MqttTimeout)) {
							var topic = $"sensate/trigger/{trigger.SensorId}";
							tasks.Add(this.m_publisher.PublishOnAsync(topic, body, false));
						}
						break;

					case TriggerActionChannel.HttpPost:
					case TriggerActionChannel.HttpGet:
						var result = Uri.TryCreate(action.Target, UriKind.Absolute, out var output) &&
									  output.Scheme == Uri.UriSchemeHttp || output.Scheme == Uri.UriSchemeHttps;

						if(!result) {
							break;
						}

						if(!CanExecute(last, this.m_timeout.HttpTimeout)) {
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

						var actuator = this.m_mqttSettings.ActuatorTopic.Replace("$sensorId", action.Target);

						var io = new[] {
								this.m_publisher.PublishOnAsync(actuator, body, false),
								this.m_conrol.CreateAsync(msg)
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
	}
}