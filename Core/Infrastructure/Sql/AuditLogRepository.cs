/*
 * AuditLog repository implementation.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class AuditLogRepository : AbstractSqlRepository<AuditLog>, IAuditLogRepository
	{
		public AuditLogRepository(SensateSqlContext ctx) : base(ctx)
		{
		}

		public async Task CreateAsync(string route, RequestMethod method, IPAddress address, SensateUser user = null)
		{
			AuditLog al;

			al = new AuditLog {
				AuthorId = user?.Id,
				Route = route,
				Method = method,
				Address = address,
				Timestamp = DateTime.Now,
			};

			await this.CreateAsync(al).AwaitBackground();
		}

		public async Task DeleteBetweenAsync(SensateUser user, DateTime start, DateTime end)
		{
			var data = from log in this.Data
					   where log.AuthorId == user.Id &&
							 log.Timestamp >= start &&
							 log.Timestamp <= end
					   select log;

			this.Data.RemoveRange(data);
			await this._sqlContext.SaveChangesAsync().AwaitBackground();
		}

		public async Task DeleteBetweenAsync(SensateUser user, string route, DateTime start, DateTime end)
		{
			var data = from log in this.Data
					   where log.AuthorId == user.Id &&
							 log.Route == route &&
							 log.Timestamp >= start &&
							 log.Timestamp <= end
					   select log;

			this.Data.RemoveRange(data);
			await this._sqlContext.SaveChangesAsync().AwaitBackground();
		}

		public async Task<IEnumerable<AuditLog>> GetByRouteAsync(SensateUser user, string route, DateTime start, DateTime end)
		{
			var data = from log in this.Data
					   where log.AuthorId == user.Id &&
							 log.Route == route &&
							 log.Timestamp >= start &&
							 log.Timestamp <= end
					   select log;

			var result = await data.ToListAsync().AwaitBackground();
			return result;
		}

		public async Task<IEnumerable<AuditLog>> GetByRequestTypeAsync(SensateUser user, RequestMethod method)
		{
			var data = from log in this.Data
					   where log.AuthorId == user.Id &&
							 log.Method == method
					   select log;

			var result = await data.ToListAsync().AwaitBackground();
			return result;
		}

		public async Task<IEnumerable<AuditLog>> GetAsync(Expression<Func<AuditLog, bool>> expr)
		{
			var data = this.Data.Where(expr);
			var result = await data.ToListAsync().AwaitBackground();

			return result;
		}

		public Task<AuditLog> GetAsync(long id)
		{
			var data = this.Data.Where(x => x.Id == id);

			return data.FirstOrDefaultAsync();
		}

		public async Task<IEnumerable<AuditLog>> GetBetweenAsync(SensateUser user, DateTime start, DateTime end)
		{
			var data = from log in this.Data
					   where log.AuthorId == user.Id &&
							 log.Timestamp >= start &&
							 log.Timestamp <= end
					   select log;

			var result = await data.ToListAsync().AwaitBackground();
			return result;
		}

		public async Task<IEnumerable<AuditLog>> GetByRouteAsync(SensateUser user, string route)
		{
			var data = from log in this.Data
					   where log.AuthorId == user.Id &&
							 log.Route == route
					   select log;

			var result = await data.ToListAsync().AwaitBackground();
			return result;
		}

		public Task<int> CountAsync(Expression<Func<AuditLog, bool>> predicate)
		{
			return this.Data.CountAsync(predicate);
		}

		public async Task<IEnumerable<AuditLog>> GetByUserAsync(SensateUser user)
		{
			var data = this.Data.Where(x => x.AuthorId == user.Id);
			return await data.ToListAsync().AwaitBackground();
		}
	}
}
