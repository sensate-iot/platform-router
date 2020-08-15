/*
 * Cached measurement store interface definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;

namespace SensateService.Infrastructure.Storage
{
	public interface ICachedMeasurementStore : IMeasurementCache
	{
		Task<long> ProcessMeasurementsAsync();
		void Destroy();
	}
}
