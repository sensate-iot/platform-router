/*
 * Geo query service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Driver.GeoJsonObjectModel;

using SensateService.Models.Generic;

namespace SensateService.Services
{
	public interface IGeoQueryService
	{
		IList<MeasurementsQueryResult> GetMeasurementsNear(
			List<MeasurementsQueryResult> measurements, GeoJson2DGeographicCoordinates coords,
			int radius = 100, int skip = -1, int limit = -1, CancellationToken ct = default
		);
	}
}
