﻿/*
 * Blob repository interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SensateService.Common.Data.Dto.Json.Out;
using SensateService.Common.Data.Enums;
using SensateService.Common.Data.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IBlobRepository
	{
		Task CreateAsync(Blob blob, CancellationToken ct = default);

		Task<Blob> GetAsync(long blobId, CancellationToken ct = default);
		Task<PaginationResult<Blob>> GetAsync(string sensorId, int skip = -1, int limit = -1, OrderDirection order = OrderDirection.None, CancellationToken ct = default);
		Task<PaginationResult<Blob>> GetRangeAsync(IList<Sensor> sensors, int skip = -1, int limit = -1, OrderDirection order = OrderDirection.None, CancellationToken ct = default);
		Task<IEnumerable<Blob>> GetAsync(IList<Sensor> sensors, DateTime start, DateTime end, int skip = -1, int limit = -1, OrderDirection order = OrderDirection.None, CancellationToken ct = default);
		Task<PaginationResult<Blob>> GetLikeAsync(string sensorId, string fileName, int skip = -1, int limit = -1, OrderDirection order = OrderDirection.None, CancellationToken ct = default);
		Task<Blob> GetAsync(string sensorId, string fileName, CancellationToken ct = default);

		Task<bool> DeleteAsync(string sensorId, string fileName, CancellationToken ct = default);
		Task<bool> DeleteAsync(long id, CancellationToken ct = default);
		Task DeleteAsync(Sensor sensor, CancellationToken ct = default);
	}
}