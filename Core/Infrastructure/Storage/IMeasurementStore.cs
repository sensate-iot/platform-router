/*
 * Measurement store interface definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;

using SensateService.Enums;

namespace SensateService.Infrastructure.Storage
{
	public interface IMeasurementStore
	{
		Task StoreAsync(string obj, RequestMethod method);
	}
}
