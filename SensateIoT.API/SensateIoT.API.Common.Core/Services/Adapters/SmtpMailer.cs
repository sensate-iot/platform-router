/*
 * Email sending service using SMTP.
 * 
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SensateIoT.API.Common.Config.Settings;
using SensateIoT.API.Common.Core.Helpers;
using SensateIoT.API.Common.Data.Dto.Generic;

namespace SensateIoT.API.Common.Core.Services.Adapters
{
	public class SmtpMailer : IEmailSender
	{
		private readonly SmtpAuthOptions _options;
		private readonly SmtpClient _client;

		public SmtpMailer(IOptions<SmtpAuthOptions> options)
		{
			this._options = options.Value;
			this._client = new SmtpClient(this._options.Host, this._options.Port) {
				EnableSsl = this._options.Ssl,
				Credentials = new NetworkCredential(this._options.Username, this._options.Password, ""),
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

			await this._client.SendMailAsync(msg).AwaitBackground();
		}

		public async Task SendEmailAsync(string recip, string subj, string body)
		{
			var msg = new EmailBody {
				HtmlBody = body
			};

			await this.SendEmailAsync(recip, subj, msg).AwaitBackground();
		}
	}
}
