/*
 * Measurement cache interface definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading.Tasks;

using SensateService.Enums;
using SensateService.Models.Json.In;

namespace SensateService.Infrastructure.Storage
{
	public interface IMeasurementCache
	{
		Task StoreAsync(RawMeasurement obj, RequestMethod methodd);
		Task StoreRangeAsync(IEnumerable<RawMeasurement> measurements, RequestMethod method);
	}
}
