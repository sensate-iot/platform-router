/*
 * Abstract SQL repository
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SensateService.Helpers;

namespace SensateService.Infrastructure.Sql
{
	public abstract class AbstractSqlRepository<T> : IBulkWriter<T> where T : class
	{
		private readonly SensateSqlContext _sqlContext;
		protected DbSet<T> Data;

		protected AbstractSqlRepository(SensateSqlContext context)
		{
			this._sqlContext = context;
			this.Data = context.Set<T>();
		}

		public void Commit(T obj)
		{
			this._sqlContext.SaveChanges();
		}

		public async Task CommitAsync(T obj, CancellationToken ct = default(CancellationToken))
		{
			await this._sqlContext.SaveChangesAsync(ct).AwaitSafely();
		}

		public async Task CommitAsync(CancellationToken ct = default(CancellationToken))
		{
			await this._sqlContext.SaveChangesAsync(ct).AwaitSafely();
		}

		public virtual void Commit()
		{
			this._sqlContext.SaveChanges();
		}

		public virtual void StartUpdate(T obj)
		{
			this._sqlContext.Update(obj);
		}

		public virtual async Task EndUpdateAsync()
		{
			await this._sqlContext.SaveChangesAsync().AwaitSafely();
		}

		public void EndUpdate()
		{
			this._sqlContext.SaveChanges();
		}

		public virtual void Create(T obj)
		{
			this.Data.Add(obj);
			this.Commit();
		}

		public virtual async Task CreateAsync(T obj)
		{
			this.Data.Add(obj);
			await this.CommitAsync();
		}

		public void AddRange(IEnumerable<T> objs)
		{
			this.Data.AddRange(objs);
		}

		public virtual async Task CreateRangeAsync(IEnumerable<T> objs, CancellationToken token)
		{
			this.AddRange(objs);
			await this.CommitAsync(token);
		}
	}
}
