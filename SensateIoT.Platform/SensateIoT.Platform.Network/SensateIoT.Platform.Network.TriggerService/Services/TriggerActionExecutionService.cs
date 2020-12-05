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

using SensateIoT.Platform.Network.Common.Services.Adapters;
using SensateIoT.Platform.Network.Common.Settings;
using SensateIoT.Platform.Network.Data.DTO;
using SensateIoT.Platform.Network.DataAccess.Repositories;
using SensateIoT.Platform.Network.TriggerService.Clients;

namespace SensateIoT.Platform.Network.TriggerService.Services
{
	public class TriggerActionExecutionService : ITriggerActionExecutionService
	{
		private readonly ITextSendService m_text;
		private readonly IEmailSender m_mail;
		private readonly IHttpClientFactory m_httpFactory;
		private readonly TextServiceSettings m_textSettings;
		private readonly IControlMessageRepository m_controlMessges;
		private readonly IRouterClient m_router;

		public TriggerActionExecutionService(
			IHttpClientFactory factory,
			IRouterClient router,
			IControlMessageRepository controlMessages,
			ITextSendService text,
			IEmailSender mail,
			IOptions<TextServiceSettings> textOptions 
		)
		{
			this.m_text = text;
			this.m_mail = mail;
			this.m_textSettings = textOptions.Value;
			this.m_httpFactory = factory;
			this.m_controlMessges = controlMessages;
			this.m_router = router;
		}

		public async Task ExecuteAsync(TriggerAction action, string body)
		{

			switch(action.Channel) {
			case TriggerChannel.Email:
				var mail = new EmailBody {
					HtmlBody = body,
					TextBody = body
				};

				await this.m_mail.SendEmailAsync(action.Target, "Sensate IoT trigger", mail).ConfigureAwait(false);
				break;

			case TriggerChannel.SMS:
					await this.m_text.SendAsync(this.m_textSettings.AlphaCode, action.Target, body).ConfigureAwait(false);
				break;


			case TriggerChannel.HttpPost:
			case TriggerChannel.HttpGet:
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
				if(!ObjectId.TryParse(action.Target, out var id)) {
					break;
				}

				var msg = new Data.Models.ControlMessage {
					Data = body,
					SensorId = id,
					Timestamp = DateTime.UtcNow
				};

				var io = new[] {
					this.m_router.RouteControlMessageAsync(msg),
					this.m_controlMessges.CreateAsync(msg)
				};

				await Task.WhenAll(io).ConfigureAwait(false);
				break;

			default:
				throw new ArgumentOutOfRangeException();
			}

			action.LastInvocation = DateTime.UtcNow;
		}
	}
}
