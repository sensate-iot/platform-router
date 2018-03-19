/*
 * Abstract SQL repository
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace SensateService.Infrastructure.Sql
{
	public abstract class AbstractSqlRepository<TKey, T> : IRepository<TKey, T> where T : class
	{
		private SensateSqlContext _sqlContext;
		protected DbSet<T> Data;

		public AbstractSqlRepository(SensateSqlContext context)
		{
			this._sqlContext = context;
			this.Data = context.Set<T>();
		}

		public virtual void Commit(T obj)
		{
			this._sqlContext.SaveChanges();
		}

		public async virtual Task CommitAsync(T obj)
		{
			await this._sqlContext.SaveChangesAsync();
		}

		public async virtual Task CommitAsync()
		{
			await this._sqlContext.SaveChangesAsync();
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
			await this._sqlContext.SaveChangesAsync();
		}
	}
}
