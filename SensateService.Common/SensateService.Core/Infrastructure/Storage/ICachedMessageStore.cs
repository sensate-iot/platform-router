/*
 * Message store interface definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;

namespace SensateService.Infrastructure.Storage
{
	public interface ICachedMessageStore : IMessageCache
	{
		Task<long> ProcessMessagesAsync();
		void Destroy();
	}
}