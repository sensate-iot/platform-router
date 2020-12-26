/*
 * AuditLog database coupling.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SensateService.Common.Data.Dto.Json.Out;
using SensateService.Common.Data.Enums;
using SensateService.Common.Data.Models;
using SensateService.Common.IdentityData.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IAuditLogRepository
	{
		Task<PaginationResult<AuditLog>> GetByUserAsync(SensateUser user, int skip = 0, int limit = 0);
		Task<PaginationResult<AuditLog>> GetByRequestTypeAsync(SensateUser user, RequestMethod method, int skip = 0, int limit = 0);
		Task<IEnumerable<AuditLog>> GetAsync(Expression<Func<AuditLog, bool>> expr);
		Task<AuditLog> GetAsync(long id);
		Task<PaginationResult<AuditLog>> GetAllAsync(RequestMethod method, int skip = 0, int limit = 0);

		Task<int> CountAsync(Expression<Func<AuditLog, bool>> predicate = null);

		Task<PaginationResult<AuditLog>> FindAsync(string text, RequestMethod method = RequestMethod.Any, int skip = 0, int limit = 0);
		Task<PaginationResult<AuditLog>> FindAsync(IEnumerable<string> uids, string text, RequestMethod method = RequestMethod.Any, int skip = 0, int limit = 0);

		Task CreateAsync(string route, RequestMethod method, IPAddress address, SensateUser user = null);
		Task CreateAsync(AuditLog log, CancellationToken ct = default);

		Task DeleteBetweenAsync(SensateUser user, DateTime start, DateTime end);
		Task DeleteBetweenAsync(SensateUser user, string route, DateTime start, DateTime end);
		Task DeleteAsync(IEnumerable<long> ids, CancellationToken ct = default);
	}
}
