/*
 * Geo query service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;

using MongoDB.Driver.GeoJsonObjectModel;
using SensateService.Common.Data.Enums;
using SensateService.Models.Generic;

namespace SensateService.Services
{
	public interface IGeoQueryService
	{
		IList<MeasurementsQueryResult> GetMeasurementsNear(
			List<MeasurementsQueryResult> measurements, GeoJson2DGeographicCoordinates coords,
			int radius = 100, int skip = -1, int limit = -1,
			OrderDirection order = OrderDirection.None, CancellationToken ct = default
		);
	}
}
