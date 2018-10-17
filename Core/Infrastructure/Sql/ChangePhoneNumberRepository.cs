/*
 * Change phone number token repository implementation.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SensateService.Exceptions;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class ChangePhoneNumberRepository : AbstractSqlRepository<string, ChangePhoneNumberToken>, IChangePhoneNumberTokenRepository
	{
		private Random _rng;
		private const int UserTokenLength = 6;

		public ChangePhoneNumberRepository(SensateSqlContext context) : base(context)
		{
			this._rng = new Random();
		}

		public async Task<string> CreateAsync(SensateUser user, string token, string phone)
		{
			ChangePhoneNumberToken t;

			t = new ChangePhoneNumberToken {
				PhoneNumber = phone,
				IdentityToken = token,
				UserToken = this._rng.NextString(UserTokenLength),
				User = user,
				Timestamp = DateTime.Now
			};

			try {
				await this.CreateAsync(t).AwaitSafely();
			} catch(Exception e) {
				Debug.WriteLine($"Unable to create token: {e.Message}");
				return null;
			}

			return t.UserToken;
		}

		public override ChangePhoneNumberToken GetById(string id)
		{
			return this.Data.FirstOrDefault(x => x.UserToken == id);
		}

		public ChangePhoneNumberToken GetLastByUser(SensateUser user)
		{
			throw new NotImplementedException();
		}


		public override async Task CreateAsync(ChangePhoneNumberToken obj)
		{
			this.Data.Add(obj);
			await this.CommitAsync().AwaitSafely();
		}

		public override void Create(ChangePhoneNumberToken obj)
		{
			this.Data.Add(obj);
			this.Commit();
		}

		public override void Update(ChangePhoneNumberToken obj)
		{
			throw new NotAllowedException("Not allowed to update phone number token!");
		}

		public override void Delete(string id)
		{
			throw new NotAllowedException("Not allowed to delete phone number token!");
		}

		public override Task DeleteAsync(string id)
		{
			throw new NotAllowedException("Not allowed to delete phone number token!");
		}
	}
}