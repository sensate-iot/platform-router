/*
 * User repository interface.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System.Threading.Tasks;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class UserRepository : AbstractSqlRepository<string, SensateUser>, IUserRepository
	{
		public UserRepository(SensateSqlContext context) : base(context)
		{
		}

		public override void Commit(SensateUser obj)
		{
			throw new System.NotImplementedException();
		}

		public override Task CommitAsync(SensateUser obj)
		{
			throw new System.NotImplementedException();
		}

		public override void Create(SensateUser obj)
		{
			throw new System.NotImplementedException();
		}

		public override bool Delete(string id)
		{
			throw new System.NotImplementedException();
		}

		public override SensateUser GetById(string id)
		{
			throw new System.NotImplementedException();
		}

		public SensateUser Get(string key)
		{
			return this.GetById(key);
		}

		public async Task<SensateUser> GetAsync(string key)
		{
			var user = await Task.Run(() => {
				return this.GetById(key);
			});

			return user;
		}

		public override bool Replace(SensateUser obj1, SensateUser obj2)
		{
			throw new System.NotImplementedException();
		}

		public override bool Update(SensateUser obj)
		{
			throw new System.NotImplementedException();
		}
	}
}
