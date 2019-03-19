/*
 * Measurement cache interface definition.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using SensateService.Enums;
using SensateService.Models.Json.In;

namespace SensateService.Infrastructure.Storage
{
	public interface IMeasurementCache
	{
		Task StoreAsync(RawMeasurement obj, RequestMethod method = RequestMethod.Any);
		Task StoreRangeAsync(IEnumerable<RawMeasurement> measurements, RequestMethod method = RequestMethod.Any);
	}
}
