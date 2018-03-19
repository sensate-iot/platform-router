/*
 * Email sender service using SendGrind.com.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

using SendGrid;
using SendGrid.Helpers.Mail;

namespace SensateService.Services
{
	public class EmailSender : IEmailSender
	{
		private MessageSenderAuthOptions _options;

		public EmailSender(IOptions<MessageSenderAuthOptions> opts)
		{
			this._options = opts.Value;
		}

		public async Task SendEmailAsync(string recip, string subj, string body)
		{
			await this.Execute(this._options.Key, recip, subj, body);
		}

		private Task<Response> Execute(string key, string recip, string subj, string body)
		{
			SendGridMessage msg;
			var client = new SendGridClient(key);

			msg = new SendGridMessage() {
				From = new EmailAddress(this._options.From, this._options.FromName),
				Subject = subj,
				PlainTextContent = body,
				HtmlContent = body,
			};

			msg.AddTo(recip);
			return client.SendEmailAsync(msg);
		}
	}
}
