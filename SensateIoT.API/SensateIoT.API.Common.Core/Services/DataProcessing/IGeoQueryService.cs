/*
 * Geo query service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading;

using SensateIoT.API.Common.Data.Dto.Generic;
using SensateIoT.API.Common.Data.Enums;

using MeasurementsQueryResult = SensateIoT.API.Common.Data.Models.MeasurementsQueryResult;

namespace SensateIoT.API.Common.Core.Services.DataProcessing
{
	public interface IGeoQueryService
	{
		IList<MeasurementsQueryResult> GetMeasurementsNear(List<MeasurementsQueryResult> measurements,
														   GeoJsonPoint coords,
														   int radius = 100,
														   int skip = -1,
														   int limit = -1,
														   OrderDirection order = OrderDirection.None,
														   CancellationToken ct = default
		);
	}
}
