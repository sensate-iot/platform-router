/*
 * Abstract document repository
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Driver;
using MongoDB.Bson;

namespace SensateService.Models.Database
{
	public abstract class AbstractDocumentRepository<T> : IRepository<T> where T : class
	{
		private readonly SensateContext context;

		public AbstractDocumentRepository(SensateContext context)
		{
			this.context = context;
		}

		public abstract void Commit(T obj);
		public abstract Task CommitAsync(T obj);
		public abstract bool Create(T obj);
		public abstract bool Delete(long id);
		public abstract T GetById(long id);
		public abstract bool Replace(T obj1, T obj2);
		public abstract bool Update(T obj);


		protected ObjectId ToInternalId(long id)
		{
			ObjectId internalId;

			if(!ObjectId.TryParse(id.ToString(), out internalId))
				internalId = ObjectId.Empty;

			return internalId;
		}
	}
}
