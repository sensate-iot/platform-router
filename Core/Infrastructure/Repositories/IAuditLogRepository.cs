﻿/*
 * AuditLog database coupling.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using SensateService.Enums;
using SensateService.Models;

namespace SensateService.Infrastructure.Repositories
{
    public interface IAuditLogRepository
    {
		IEnumerable<AuditLog> GetByUser(SensateUser user);
		Task<IEnumerable<AuditLog>> GetByUserAsync(SensateUser user);

		IEnumerable<AuditLog> GetBetween(DateTime start, DateTime end);
		Task<IEnumerable<AuditLog>> GetBetweenAsync(DateTime start, DateTime end);

		IEnumerable<AuditLog> GetByRoute(string route);
		Task<IEnumerable<AuditLog>> GetByRouteAsync(string route);

		AuditLog Get(long id);
		AuditLog GetById(long id);
		Task<AuditLog> GetAsync(long id);

		void Create(string route, RequestMethod method, IPAddress address,  SensateUser user = null);
		Task CreateAsync(string route, RequestMethod method, IPAddress address, SensateUser user = null);
    }
}