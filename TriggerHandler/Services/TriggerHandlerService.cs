/*
 * Trigger handling service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using MongoDB.Bson;

using SensateService.Common.Data.Models;
using SensateService.Common.IdentityData.Models;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models.Generic;
using SensateService.Services;
using SensateService.Services.Settings;
using SensateService.TriggerHandler.Application;
using SensateService.TriggerHandler.Models;

namespace SensateService.TriggerHandler.Services
{
	public class TriggerHandlerService : ITriggerHandlerService
	{
		private readonly IControlMessageRepository m_conrol;
		private readonly ITextSendService m_text;
		private readonly IEmailSender m_mail;
		private readonly IMqttPublishService m_publisher;
		private readonly TimeoutSettings m_timeout;
		private readonly MqttPublishServiceOptions m_mqttSettings;
		private readonly TextServiceSettings m_textSettings;

		public TriggerHandlerService(
			IControlMessageRepository conrol,
			ITextSendService text,
			IEmailSender mail,
			IMqttPublishService publisher,
			IOptions<TextServiceSettings> text_opts,
			IOptions<MqttPublishServiceOptions> options,
			IOptions<TimeoutSettings> timeout
			)
		{
			this.m_conrol = conrol;
			this.m_text = text;
			this.m_mail = mail;
			this.m_publisher = publisher;
			this.m_mqttSettings = options.Value;
			this.m_timeout = timeout.Value;
			this.m_textSettings = text_opts.Value;
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

		public async Task HandleTriggerAction(SensateUser user, Trigger trigger, TriggerAction action, TriggerInvocation last, string body)
		{
			var client = new RestClient();

			switch(action.Channel) {
			case TriggerActionChannel.Email:
				if(!user.EmailConfirmed) {
					break;
				}

				if(CanExecute(last, this.m_timeout.MailTimeout)) {
					var mail = new EmailBody {
						HtmlBody = body,
						TextBody = body
					};

					await this.m_mail.SendEmailAsync(user.Email, "Sensate trigger triggered", mail).AwaitBackground();
				}

				break;
			case TriggerActionChannel.SMS:
				if(CanExecute(last, this.m_timeout.MessageTimeout)) {
					if(!user.PhoneNumberConfirmed)
						break;

					await this.m_text.SendAsync(this.m_textSettings.AlphaCode, user.PhoneNumber, body).AwaitBackground();
				}

				break;

			case TriggerActionChannel.MQTT:
				if(CanExecute(last, this.m_timeout.MqttTimeout)) {
					var topic = $"sensate/trigger/{trigger.SensorId}";
					await this.m_publisher.PublishOnAsync(topic, body, false).AwaitBackground();
				}
				break;

			case TriggerActionChannel.HttpPost:
			case TriggerActionChannel.HttpGet:
				var result = Uri.TryCreate(action.Target, UriKind.Absolute, out var output) &&
							  output.Scheme == Uri.UriSchemeHttp || output?.Scheme == Uri.UriSchemeHttps;

				if(!result) {
					break;
				}

				if(!CanExecute(last, this.m_timeout.HttpTimeout)) {
					break;
				}

				var t = action.Channel == TriggerActionChannel.HttpGet
					? client.GetAsync(action.Target)
					: client.PostAsync(action.Target, body);
				await t.AwaitBackground();
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
}