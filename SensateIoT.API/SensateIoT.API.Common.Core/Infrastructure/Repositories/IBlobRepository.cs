/*
 * Blob repository interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SensateIoT.API.Common.Data.Enums;
using SensateIoT.API.Common.Data.Models;

namespace SensateIoT.API.Common.Core.Infrastructure.Repositories
{
	public interface IBlobRepository
	{
		Task<IEnumerable<Blob>> GetAsync(IList<Sensor> sensors, DateTime start, DateTime end, int skip = -1, int limit = -1, OrderDirection order = OrderDirection.None, CancellationToken ct = default);
	}
}
