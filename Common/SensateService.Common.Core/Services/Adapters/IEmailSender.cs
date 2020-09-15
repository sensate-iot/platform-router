/*
 * SMTP email interface.
 * 
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Threading.Tasks;
using SensateService.Common.Data.Dto.Generic;

namespace SensateService.Services.Adapters
{
	public interface IEmailSender
	{
		Task SendEmailAsync(string recip, string subj, EmailBody body);
		Task SendEmailAsync(string recip, string subj, string body);
	}
}
