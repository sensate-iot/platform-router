/*
 * Cached measurement store interface definition.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Threading.Tasks;

namespace SensateService.Infrastructure.Storage
{
	public interface ICachedMeasurementStore : IMeasurementCache
	{
		Task<long> ProcessAsync();
		void Destroy();
	}
}
