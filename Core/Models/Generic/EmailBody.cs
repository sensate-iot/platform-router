/*
 * Email body.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Collections.Generic;
using SendGrid.Helpers.Mail;

namespace SensateService.Models.Generic
{
	public class EmailBody
	{
		public string HtmlBody { get; set; }
		public string TextBody { get; set; }
		public string FromEmail { get; set; }
		public string FromName { get; set; }
		public string Subject { get; set; }

		private readonly List<EmailAddress> _recipients;

		public EmailBody() => this._recipients = new List<EmailAddress>();

		public void AddRecip(string mail) => this._recipients.Add(new EmailAddress(mail));

		public SendGridMessage BuildSendgridMessage()
		{
			var msg = new SendGridMessage {
				HtmlContent = this.HtmlBody,
				PlainTextContent = this.TextBody,
				From = new EmailAddress(FromEmail, FromName),
				Subject = Subject
			};

			msg.AddTos(this._recipients);

			return msg;
		}
	}
}
