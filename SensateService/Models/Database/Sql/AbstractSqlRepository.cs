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

namespace SensateService.Models.Database.Sql
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

		public abstract bool Create(T obj);
		public abstract bool Replace(T obj1, T obj2);
		public abstract bool Update(T obj);
		public abstract void Commit(T obj);
		public abstract Task CommitAsync(T obj);
		public abstract T GetById(TKey id);
		public abstract bool Delete(TKey id);
	}
}
