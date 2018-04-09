/*
 * Email sending service using SMTP.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Net;
using System.Threading.Tasks;
using System.Net.Mail;

using Microsoft.Extensions.Options;

namespace SensateService.Services
{
	public class SmtpMailer : IEmailSender
	{
		private readonly SmtpAuthOptions _options;
		private readonly SmtpClient _client;

		public SmtpMailer(IOptions<SmtpAuthOptions> options)
		{
			this._options = options.Value;
			this._client = new SmtpClient(_options.Host, _options.Port) {
				EnableSsl = _options.Ssl,
				Credentials = new NetworkCredential(_options.Username, _options.Password, ""),
				Timeout = 3000
			};
		}

		public async Task SendEmailAsync(string recip, string subj, EmailBody body)
		{
			var msg = new MailMessage {
				From = new MailAddress(this._options.From, this._options.FromName),
				IsBodyHtml = true,
				Body = body.HtmlBody,
				Subject = subj
			};

			msg.To.Add(recip);
			await Task.Run(() => this._client.Send(msg));
		}

		public async Task SendEmailAsync(string recip, string subj, string body)
		{
			var msg = new EmailBody {
				HtmlBody = body
			};

			await this.SendEmailAsync(recip, subj, msg);
		}
	}
}
