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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Models.Json.Out;

namespace SensateService.Infrastructure.Sql
{
	public class AuditLogRepository : AbstractSqlRepository<AuditLog>, IAuditLogRepository
	{
		public AuditLogRepository(SensateSqlContext ctx) : base(ctx)
		{
		}

		public async Task<PaginationResult<AuditLog>> FindAsync(string text, RequestMethod method = RequestMethod.Any, int skip = 0, int limit = 0)
		{
			IQueryable<AuditLog> query;
			var rv = new PaginationResult<AuditLog>();

			if(text == null) {
				return await this.GetAllAsync(method, skip, limit).AwaitBackground();
			}

			if(method != RequestMethod.Any) {
				query = this.Data.Where(log => log.Route.Contains(text) && log.Method == method);
			} else {
				query = this.Data.Where(log => log.Route.Contains(text));
			}

			rv.Count = await query.CountAsync().AwaitBackground();

			if(skip > 0) {
				query = query.Skip(skip);
			}

			if(limit > 0) {
				query = query.Take(limit);
			}

			rv.Values = await query.ToListAsync().AwaitBackground();

			foreach(var log in rv.Values) {
				log.Timestamp = DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc);
			}

			return rv;
		}

		public async Task<PaginationResult<AuditLog>> FindAsync(
			IEnumerable<string> uids,
			string text,
			RequestMethod method = RequestMethod.Any,
			int skip = 0,
			int limit = 0
			)
		{
			IQueryable<AuditLog> logs;
			var rv = new PaginationResult<AuditLog>();

			if(method == RequestMethod.Any) {
				if(text == null) {
					logs =
						from log in this.Data
						where uids.Contains(log.AuthorId)
						select log;
				} else {
					logs =
						from log in this.Data
						where uids.Contains(log.AuthorId) &&
							  log.Route.Contains(text)
						select log;
				}
			} else {
				if(text == null) {
					logs = this.Data.Where(log => uids.Contains(log.AuthorId) && log.Method == method);
				} else {
					logs = this.Data.Where(log => uids.Contains(log.AuthorId) && log.Method == method && log.Route.Contains(text));
				}
			}

			rv.Count = await logs.CountAsync().AwaitBackground();

			if(skip > 0) {
				logs = logs.Skip(skip);
			}

			if(limit > 0) {
				logs = logs.Take(limit);
			}

			rv.Values = await logs.ToListAsync().AwaitBackground();

			foreach(var log in rv.Values) {
				log.Timestamp = DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc);
			}

			return rv;
		}

		public async Task<IEnumerable<AuditLog>> FindAsync(SensateUser user, string text, int skip = 0, int limit = 0)
		{
			var query = this.Data.Where(log => log.AuthorId == user.Id && log.Route.Contains(text));

			if(skip > 0) {
				query = query.Skip(skip);
			}

			if(limit > 0) {
				query = query.Take(limit);
			}

			var result = await query.ToListAsync().AwaitBackground();

			foreach(var log in result) {
				log.Timestamp = DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc);
			}

			return result;
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

		public async Task DeleteAsync(IEnumerable<long> ids, CancellationToken ct = default)
		{
			var data =
				from log in this.Data
				where ids.Contains(log.Id)
				select log;

			this.Data.RemoveRange(data);
			await this.CommitAsync(ct).AwaitBackground();
		}

		public async Task<IEnumerable<AuditLog>> GetByRouteAsync(SensateUser user, string route, DateTime start, DateTime end, int skip = 0, int limit = 0)
		{
			var result =
				from log in this.Data
				where log.AuthorId == user.Id &&
					  log.Route == route &&
					  log.Timestamp >= start &&
					  log.Timestamp <= end
				orderby log.Timestamp
				select log;

			var data = result.AsQueryable();
			if(skip > 0) {
				data = data.Skip(skip);
			}

			if(limit > 0) {
				data = data.Take(limit);
			}

			var rv = await data.ToListAsync().AwaitBackground();

			foreach(var log in result) {
				log.Timestamp = DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc);
			}

			return rv;
		}

		public async Task<PaginationResult<AuditLog>> GetByRequestTypeAsync(SensateUser user, RequestMethod method, int skip = 0, int limit = 0)
		{
			var rv = new PaginationResult<AuditLog>();

			var query =
				from log in this.Data
				where log.AuthorId == user.Id &&
					  log.Method == method
				orderby log.Timestamp
				select log;
			var data = query.AsQueryable();

			rv.Count = await data.CountAsync().AwaitBackground();

			if(skip > 0) {
				data = data.Skip(skip);
			}

			if(limit > 0) {
				data = data.Take(limit);
			}

			rv.Values = await data.ToListAsync().AwaitBackground();

			foreach(var log in rv.Values) {
				log.Timestamp = DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc);
			}

			return rv;
		}

		public async Task<IEnumerable<AuditLog>> GetAsync(Expression<Func<AuditLog, bool>> expr)
		{
			var data = this.Data.Where(expr);
			var result = await data.ToListAsync().AwaitBackground();

			foreach(var log in result) {
				log.Timestamp = DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc);
			}

			return result;
		}

		public async Task<AuditLog> GetAsync(long id)
		{
			var data = this.Data.Where(x => x.Id == id);
			var log = await data.FirstOrDefaultAsync().AwaitBackground();

			log.Timestamp = DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc);
			return log;
		}

		public async Task<PaginationResult<AuditLog>> GetAllAsync(RequestMethod method, int skip = 0, int limit = 0)
		{
			IQueryable<AuditLog> result;
			var rv = new PaginationResult<AuditLog>();

			if(method == RequestMethod.Any) {
				result = this.Data.AsQueryable();
			} else {
				result = this.Data.Where(log => log.Method == method);
			}

			rv.Count = await result.CountAsync().AwaitBackground();

			if(skip > 0) {
				result = result.Skip(skip);
			}

			if(limit > 0) {
				result = result.Take(limit);
			}

			rv.Values = await result.ToListAsync().AwaitBackground();

			foreach(var log in rv.Values) {
				log.Timestamp = DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc);
			}

			return rv;
		}

		public async Task<IEnumerable<AuditLog>> GetBetweenAsync(SensateUser user, DateTime start, DateTime end, int skip = 0, int limit = 0)
		{
			var query =
				from log in this.Data
				where log.AuthorId == user.Id &&
					  log.Timestamp >= start &&
					  log.Timestamp <= end
				orderby log.Timestamp
				select log;
			var data = query.AsQueryable();

			if(skip > 0) {
				data = data.Skip(skip);
			}

			if(limit > 0) {
				data = data.Take(limit);
			}

			var result = await data.ToListAsync().AwaitBackground();

			foreach(var log in result) {
				log.Timestamp = DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc);
			}

			return result;
		}

		public async Task<IEnumerable<AuditLog>> GetByRouteAsync(SensateUser user, string route, int skip = 0, int limit = 0)
		{
			var query =
				from log in this.Data
				where log.AuthorId == user.Id &&
					  log.Route == route
				orderby log.Timestamp
				select log;
			var data = query.AsQueryable();

			if(skip > 0) {
				data = data.Skip(skip);
			}

			if(limit > 0) {
				data = data.Take(limit);
			}

			var result = await data.ToListAsync().AwaitBackground();

			foreach(var log in result) {
				log.Timestamp = DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc);
			}

			return result;
		}

		public Task<int> CountAsync(Expression<Func<AuditLog, bool>> predicate = null)
		{
			if(predicate == null) {
				return this.Data.CountAsync();
			}

			return this.Data.CountAsync(predicate);
		}

		public async Task<PaginationResult<AuditLog>> GetByUserAsync(SensateUser user, int skip = 0, int limit = 0)
		{
			var data = this.Data.Where(x => x.AuthorId == user.Id);
			var rv = new PaginationResult<AuditLog> {
				Count = await data.CountAsync().AwaitBackground()
			};

			if(skip > 0) {
				data = data.Skip(skip);
			}

			if(limit > 0) {
				data = data.Take(limit);
			}

			rv.Values = await data.ToListAsync().AwaitBackground();

			foreach(var log in rv.Values) {
				log.Timestamp = DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc);
			}

			return rv;
		}
	}
}
