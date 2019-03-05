/*
 * AuditLog repository implementation.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SensateService.Enums;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Document
{
	public class AuditLogRepository : AbstractDocumentRepository<AuditLog>, IAuditLogRepository, IBulkWriter<AuditLog>
	{
		private static readonly Random Rng = new Random();

		public AuditLogRepository(SensateContext ctx) : base(ctx.Logs)
		{
		}

		public async Task CreateAsync(string route, RequestMethod method, IPAddress address, SensateUser user = null)
		{
			AuditLog al;

			al = new AuditLog() {
				AuthorId = user?.Id,
				Route = route,
				Method = method,
				Address = address,
				Timestamp = DateTime.Now,
				InternalId = base.GenerateId(DateTime.Now)
			};

			await this.CreateAsync(al).AwaitBackground();
		}

		public async Task<IEnumerable<AuditLog>> GetByRequestTypeAsync(SensateUser user, RequestMethod method)
		{
			var worker = this._collection.FindAsync(log => log.AuthorId == user.Id && log.Method == method);
			var results = await worker.AwaitBackground();
			return results.ToList();
		}

		public async Task<AuditLog> GetAsync(string id)
		{
			var worker = this._collection.FindAsync(log => log.InternalId.ToString() == id);
			var results = await worker.AwaitBackground();
			return results.SingleOrDefault();
		}

		public async Task<IEnumerable<AuditLog>> GetBetweenAsync(SensateUser user, DateTime start, DateTime end)
		{
			var worker = this._collection.FindAsync(log => log.AuthorId == user.Id &&
			                                               log.Timestamp >= start && log.Timestamp <= end);
			var results = await worker.AwaitBackground();
			return results.ToList();
		}

		public async Task<IEnumerable<AuditLog>> GetByRouteAsync(SensateUser user, string route)
		{
			var worker = this._collection.FindAsync(log => log.AuthorId == user.Id && log.Route == route);
			var results = await worker.AwaitBackground();
			return results.ToList();
		}

		public async Task<long> CountAsync(Expression<Func<AuditLog, bool>> predicate)
		{
			var worker = this._collection.CountDocumentsAsync(predicate);
			var result = await worker.AwaitBackground();
			return result;
		}

		public async Task<IEnumerable<AuditLog>> GetByUserAsync(SensateUser user)
		{
			var worker = this._collection.FindAsync(log => log.AuthorId == user.Id);
			var results = await worker.AwaitBackground();
			return results.ToList();
		}

		public Task CreateRangeAsync(IEnumerable<AuditLog> objs, CancellationToken token = default(CancellationToken))
		{
			var objects = objs.ToList();

			foreach(var log in objects) {
				log.InternalId = ObjectId.GenerateNewId(DateTime.Now);
			}

			return this._collection.InsertManyAsync(objects, null, token);
		}
	}
}
