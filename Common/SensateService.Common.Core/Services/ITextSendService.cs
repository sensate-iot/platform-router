/*
 * SMS sender service.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Threading.Tasks;

namespace SensateService.Services
{
	public interface ITextSendService
	{
		Task SendAsync(string id, string to, string body, bool retry = true);
		void Send(string id, string to, string body, bool retry = true);
		Task<bool> IsValidNumber(string number);
	}
}