/*
 * Execute trigger actions.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using MongoDB.Bson;
using Prometheus;

using SensateIoT.Platform.Network.Adapters.Abstract;
using SensateIoT.Platform.Network.Data.Abstract;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.DataAccess.Abstract;
using SensateIoT.Platform.Network.TriggerService.Abstract;

using TriggerAction = SensateIoT.Platform.Network.Data.DTO.TriggerAction;

namespace SensateIoT.Platform.Network.TriggerService.Services
{
	public class TriggerActionExecutionService : ITriggerActionExecutionService
	{
		private readonly ITextSendService m_text;
		private readonly IEmailSender m_mail;
		private readonly ITriggerRepository m_triggerRepo;
		private readonly IHttpClientFactory m_httpFactory;
		private readonly TextServiceSettings m_textSettings;
		private readonly IControlMessageRepository m_controlMessges;
		private readonly IRouterClient m_router;

		private readonly Counter m_smsCounter;
		private readonly Counter m_actuatorCounter;
		private readonly Counter m_emailCounter;
		private readonly Counter m_httpCounter;

		public TriggerActionExecutionService(
			IHttpClientFactory factory,
			IRouterClient router,
			IControlMessageRepository controlMessages,
			ITextSendService text,
			IEmailSender mail,
			ITriggerRepository triggers,
			IOptions<TextServiceSettings> textOptions
		)
		{
			this.m_triggerRepo = triggers;
			this.m_text = text;
			this.m_mail = mail;
			this.m_textSettings = textOptions.Value;
			this.m_httpFactory = factory;
			this.m_controlMessges = controlMessages;
			this.m_router = router;

			this.m_actuatorCounter = Metrics.CreateCounter("triggerservice_actuator_total", "Total number of actuator messages sent.");
			this.m_smsCounter = Metrics.CreateCounter("triggerservice_sms_total", "Total number of SMS messages sent.");
			this.m_emailCounter = Metrics.CreateCounter("triggerservice_email_total", "Total number of email messages sent.");
			this.m_httpCounter = Metrics.CreateCounter("triggerservice_http_total", "Total number of HTTP requests sent.");
		}

		public async Task ExecuteAsync(TriggerAction action, string body)
		{
			switch(action.Channel) {
			case TriggerChannel.Email:
				var mail = new EmailBody {
					HtmlBody = body,
					TextBody = body
				};

				this.m_emailCounter.Inc();
				await this.m_mail.SendEmailAsync(action.Target, "Sensate IoT trigger", mail).ConfigureAwait(false);
				break;

			case TriggerChannel.SMS:
				this.m_smsCounter.Inc();
				await this.m_text.SendAsync(this.m_textSettings.AlphaCode, action.Target, body).ConfigureAwait(false);
				break;


			case TriggerChannel.HttpPost:
			case TriggerChannel.HttpGet:
				this.m_httpCounter.Inc();
				var client = this.m_httpFactory.CreateClient();
				var result = Uri.TryCreate(action.Target, UriKind.Absolute, out var output) &&
							  output.Scheme == Uri.UriSchemeHttp || output?.Scheme == Uri.UriSchemeHttps;

				if(!result) {
					break;
				}

				var t = action.Channel == TriggerChannel.HttpGet
					? client.GetAsync(action.Target)
					: client.PostAsync(action.Target, new StringContent(body));

				await t.ConfigureAwait(false);
				break;

			case TriggerChannel.MQTT:
			case TriggerChannel.ControlMessage:
			case TriggerChannel.LiveData:
				this.m_actuatorCounter.Inc();

				if(!ObjectId.TryParse(action.Target, out var id)) {
					break;
				}

				var msg = new Data.Models.ControlMessage {
					Data = body,
					SensorId = id,
					Timestamp = DateTime.UtcNow
				};

				var io = new[] {
					this.m_router.RouteControlMessageAsync(msg, action.Channel == TriggerChannel.LiveData ? ControlMessageType.LiveData : ControlMessageType.Mqtt),
					this.m_controlMessges.CreateAsync(msg)
				};

				await Task.WhenAll(io).ConfigureAwait(false);
				break;

			default:
				throw new ArgumentOutOfRangeException(nameof(TriggerAction.Channel));
			}
		}
	}
}
