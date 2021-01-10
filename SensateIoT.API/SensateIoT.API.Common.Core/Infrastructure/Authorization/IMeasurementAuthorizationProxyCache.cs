/*
 * Cached proxy for the authorization service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;

namespace SensateIoT.API.Common.Core.Infrastructure.Authorization
{
	public interface IMeasurementAuthorizationProxyCache
	{
		void AddMessage(string data);
		void AddMessages(string data);
		Task<long> ProcessAsync(string remote);
	}
}