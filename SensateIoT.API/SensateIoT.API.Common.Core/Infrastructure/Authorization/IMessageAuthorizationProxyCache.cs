/*
 * Authorization proxy cache interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;

namespace SensateIoT.API.Common.Core.Infrastructure.Authorization
{
	public interface IMessageAuthorizationProxyCache
	{
		void AddMessage(string data);
		void AddMessages(string data);
		Task<long> ProcessAsync(string remote);
	}
}