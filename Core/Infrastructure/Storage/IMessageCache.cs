/*
 * Message store interface definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;

namespace SensateService.Infrastructure.Storage
{
	public interface IMessageCache
	{
		Task StoreAsync(string obj);
	}
}