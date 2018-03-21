/*
 * Repository for the SensateUserToken table.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using SensateService.Exceptions;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using SensateService.Helpers;

namespace SensateService.Infrastructure.Sql
{
	public class SensateUserTokenRepository :
		AbstractSqlRepository<Tuple<SensateUser, string>, SensateUserToken>, ISensateUserTokenRepository
	{
		private Random _rng;

		public SensateUserTokenRepository(SensateSqlContext context) : base(context)
		{
			this._rng = new Random();
		}

		public override void Create(SensateUserToken obj)
		{
			var asyncResult = this.CreateAsync(obj);
			asyncResult.Wait();
		}

		public override async Task CreateAsync(SensateUserToken obj)
		{
			if(obj.Value == null && obj.LoginProvider == "JWTrt")
				obj.Value = this._rng.NextString(64);
			else if(obj.Value == null)
				throw new DatabaseException("User token must have a value!");

			await this.Data.AddAsync(obj);
			await this.CommitAsync(obj);
		}


		public override SensateUserToken GetById(Tuple<SensateUser, string> id) => this.GetById(id.Item1, id.Item2);

		public SensateUserToken GetById(SensateUser user, string value)
		{
			return this.Data.FirstOrDefault(
				x => x.UserId == user.Id && x.Value == value
			);
		}

		public IEnumerable<SensateUserToken> GetByUser(SensateUser user)
		{
			IEnumerable<SensateUserToken> data;

			data = from token in this.Data
				   where token.UserId == user.Id
				   select token;

			return data;
		}


		public override void Update(SensateUserToken obj)
		{
			throw new NotAllowedException("Unable to update user token!");
		}

		public override void Delete(Tuple<SensateUser, string> id)
		{
			throw new NotAllowedException("Unable to delete user token!");
		}

		public override Task DeleteAsync(Tuple<SensateUser, string> id)
		{
			throw new NotAllowedException("Unable to delete user token!");
		}
	}
}
