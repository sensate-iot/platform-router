/*
 * SMTP email interface.
 * 
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Threading.Tasks;
using SensateIoT.API.Common.Data.Dto.Generic;

namespace SensateIoT.API.Common.Core.Services.Adapters
{
	public interface IEmailSender
	{
		Task SendEmailAsync(string recip, string subj, EmailBody body);
		Task SendEmailAsync(string recip, string subj, string body);
	}
}
