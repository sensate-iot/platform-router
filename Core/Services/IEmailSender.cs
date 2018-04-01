/*
 * SMTP email interface.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Threading.Tasks;
using MimeKit;

namespace SensateService.Services
{
	public interface IEmailSender
	{
		Task SendEmailAsync(string recip, string subj, BodyBuilder body);
		Task SendEmailAsync(string recip, string subj, string body);
	}
}
