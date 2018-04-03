/*
 * Email body.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Collections.Generic;
using SendGrid.Helpers.Mail;

namespace SensateService
{
	public class EmailBody
	{
		public string HtmlBody { get; set; }
		public string TextBody { get; set; }
		public string FromEmail { get; set; }
		public string FromName { get; set; }
		public string Subject { get; set; }
		private List<EmailAddress> _recipients;

		public EmailBody() => this._recipients = new List<EmailAddress>();

		public void AddRecip(string mail) => this._recipients.Add(new EmailAddress(mail));

		public SendGridMessage BuildSendgridMessage()
		{
			var msg = new SendGridMessage();

			msg.HtmlContent = this.HtmlBody;
			msg.PlainTextContent = this.TextBody;
			msg.From = new EmailAddress(FromEmail, FromName);
			msg.Subject = Subject;
			msg.AddTos(this._recipients);

			return msg;
		}
	}
}
