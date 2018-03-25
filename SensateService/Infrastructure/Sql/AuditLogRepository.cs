/*
 * AuditLog repository implementation.
 * 
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Exceptions;
using SensateService.Enums;

namespace SensateService.Infrastructure.Sql
{
	public class AuditLogRepository : AbstractSqlRepository<long, AuditLog>, IAuditLogRepository
	{
		public AuditLogRepository(SensateSqlContext ctx) : base(ctx)
		{
		}

		public void Create(string route, RequestMethod method, SensateUser user = null)
		{
			var al = new AuditLog() {
				Author = user,
				Route = route,
				Method = method,
				Timestamp = DateTime.Now
			};

			this.Create(al);
		}

		public override void Create(AuditLog obj)
		{
			this.Data.Add(obj);
			this.Commit(obj);
		}

		public async Task CreateAsync(string route, RequestMethod method, SensateUser user = null)
		{
			AuditLog al;

			al = new AuditLog() {
				Author = user,
				Route = route,
				Method = method,
				Timestamp = DateTime.Now
			};
			await this.CreateAsync(al);
		}

		public override async Task CreateAsync(AuditLog obj)
		{
			this.Data.Add(obj);
			await this.CommitAsync(obj);
		}

		public override void Delete(long id)
		{
			throw new NotAllowedException("Not allowed to delete audit log entry");
		}

		public override Task DeleteAsync(long id)
		{
			throw new NotAllowedException("Not allowed to delete audit log entry");
		}

		public AuditLog Get(long id)
		{
			AuditLog al;

			al = (from log in this.Data.AsQueryable<AuditLog>()
				  where log.Id == id
				  select log).Single<AuditLog>();
			return al;
		}

		public async Task<AuditLog> GetAsync(long id)
		{
			return await Task.Run(() => {
				return this.Get(id);
			});
		}

		public IEnumerable<AuditLog> GetBetween(DateTime start, DateTime end)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<AuditLog>> GetBetweenAsync(DateTime start, DateTime end)
		{
			throw new NotImplementedException();
		}

		public override AuditLog GetById(long id)
		{
			return this.Get(id);
		}

		public IEnumerable<AuditLog> GetByRoute(string route)
		{
			var al = from log in this.Data
					 where log.Route == route
					 select log;
			return al;
		}

		public async Task<IEnumerable<AuditLog>> GetByRouteAsync(string route)
		{
			return await Task.Run(() => {
				return this.GetByRoute(route);
			});
		}

		public IEnumerable<AuditLog> GetByUser(SensateUser user)
		{
			var al = from log in this.Data
					 where log.Author == user
					 select log;
			return al;
		}

		public async Task<IEnumerable<AuditLog>> GetByUserAsync(SensateUser user)
		{
			return await Task.Run(() => {
				return this.GetByUser(user);
			});
		}

		public override void Update(AuditLog obj)
		{
			throw new NotAllowedException("Not allowed to update audit log entry!");
		}
	}
}
