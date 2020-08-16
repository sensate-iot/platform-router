using System.Threading.Tasks;

namespace SensateService.Infrastructure.Authorization
{
	public interface IMessageAuthorizationProxyCache
	{
		void AddMessage(string data);
		void AddMessages(string data);
		Task<long> ProcessAsync(string remote);
	}
}