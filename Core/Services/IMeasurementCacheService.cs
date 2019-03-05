/*
 * Measurement cache service interface for dependency injection.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using SensateService.Infrastructure.Storage;

namespace SensateService.Services
{
	public interface IMeasurementCacheService
	{
		IMeasurementCache Next();
		void RegisterCache(ICachedMeasurementStore store);
	}
}
