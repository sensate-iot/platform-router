/*
 * SMS sender service.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Threading.Tasks;

namespace SensateService.Services
{
	public interface ITextSendService
	{
		Task SendAsync(string id, string to, string body);
		void Send(string id, string to, string body);
		Task<bool> IsValidNumber(string number);
	}
}