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

using SensateService.Enums;
using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
	public interface IAuditLogRepository
	{
		Task<IEnumerable<AuditLog>> GetByUserAsync(SensateUser user, int skip = 0, int limit = 0);
		Task<IEnumerable<AuditLog>> GetBetweenAsync(SensateUser user, DateTime start, DateTime end, int skip = 0, int limit = 0);
		Task<IEnumerable<AuditLog>> GetByRouteAsync(SensateUser user, string route, int skip = 0, int limit = 0);
		Task<IEnumerable<AuditLog>> GetByRouteAsync(SensateUser user, string route, DateTime start, DateTime end, int skip = 0, int limit = 0);
		Task<IEnumerable<AuditLog>> GetByRequestTypeAsync(SensateUser user, RequestMethod method, int skip = 0, int limit = 0);
		Task<IEnumerable<AuditLog>> GetAsync(Expression<Func<AuditLog, bool>> expr);
		Task<AuditLog> GetAsync(long id);
		Task<IEnumerable<AuditLog>> GetAllAsync(RequestMethod method, int skip, int limit);

		Task<int> CountAsync(Expression<Func<AuditLog, bool>> predicate = null);

		Task<IEnumerable<AuditLog>> FindAsync(string text, RequestMethod method = RequestMethod.Any, int skip = 0, int limit = 0);
		Task<IEnumerable<AuditLog>> FindAsync(IEnumerable<string> uids, string text, RequestMethod method = RequestMethod.Any, int skip = 0, int limit = 0);

		Task CreateAsync(string route, RequestMethod method, IPAddress address, SensateUser user = null);
		Task CreateAsync(AuditLog log, CancellationToken ct = default(CancellationToken));

		Task DeleteBetweenAsync(SensateUser user, DateTime start, DateTime end);
		Task DeleteBetweenAsync(SensateUser user, string route, DateTime start, DateTime end);
		Task DeleteAsync(IEnumerable<long> ids, CancellationToken ct = default);
	}
}
