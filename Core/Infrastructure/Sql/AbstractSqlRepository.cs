/*
 * Abstract SQL repository
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SensateService.Helpers;

namespace SensateService.Infrastructure.Sql
{
	public abstract class AbstractSqlRepository<TKey, T> : IRepository<TKey, T> where T : class
	{
		private SensateSqlContext _sqlContext;
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

		public abstract void Create(T obj);
		public abstract void Update(T obj);
		public abstract T GetById(TKey id);
		public abstract void Delete(TKey id);
		public abstract Task CreateAsync(T obj);
		public abstract Task DeleteAsync(TKey id);

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
	}
}
