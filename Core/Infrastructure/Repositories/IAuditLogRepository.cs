/*
 * AuditLog database coupling.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
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
		Task<IEnumerable<AuditLog>> GetByUserAsync(SensateUser user);
		Task<IEnumerable<AuditLog>> GetBetweenAsync(SensateUser user, DateTime start, DateTime end);
		Task<IEnumerable<AuditLog>> GetByRouteAsync(SensateUser user, string route);
		Task<IEnumerable<AuditLog>> GetByRouteAsync(SensateUser user, string route, DateTime start, DateTime end);
	    Task<IEnumerable<AuditLog>> GetByRequestTypeAsync(SensateUser user, RequestMethod method);
	    Task<IEnumerable<AuditLog>> GetAsync(Expression<Func<AuditLog, bool>> expr);
		Task<AuditLog> GetAsync(string id);

	    Task<long> CountAsync(Expression<Func<AuditLog, bool>> predicate);


		Task CreateAsync(string route, RequestMethod method, IPAddress address, SensateUser user = null);
		Task CreateAsync(AuditLog log, CancellationToken ct = default(CancellationToken));

	    Task DeleteBetweenAsync(SensateUser user, DateTime start, DateTime end);
	    Task DeleteBetweenAsync(SensateUser user, string route, DateTime start, DateTime end);
    }
}
