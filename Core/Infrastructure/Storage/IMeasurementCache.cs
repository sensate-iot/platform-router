/*
 * Measurement cache interface definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;

namespace SensateService.Infrastructure.Storage
{
	public interface IMeasurementCache
	{
		Task StoreAsync(string obj);
	}
}
