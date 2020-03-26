/*
 * Change phone number token repository implementation.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class ChangePhoneNumberRepository : AbstractSqlRepository<ChangePhoneNumberToken>, IChangePhoneNumberTokenRepository
	{
		private readonly Random _rng;
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
				UserId = user.Id,
				Timestamp = DateTime.UtcNow
			};

			try {
				await this.CreateAsync(t).AwaitBackground();
			} catch(Exception e) {
				Debug.WriteLine($"Unable to create token: {e.Message}");
				return null;
			}

			return t.UserToken;
		}

		public ChangePhoneNumberToken GetById(string id)
		{
			var value = this.Data.FirstOrDefault(x => x.UserToken == id);

			if(value == null) {
				return null;
			}

			value.Timestamp = DateTime.SpecifyKind(value.Timestamp, DateTimeKind.Utc);

			return value;
		}

		public async Task<ChangePhoneNumberToken> GetLatest(SensateUser user)
		{
			var tokens = from token in this.Data
						 where token.UserId == user.Id &&
							   token.PhoneNumber == user.UnconfirmedPhoneNumber
						 select token;
			var single = tokens.OrderByDescending(t => t.Timestamp);

			var value = await single.FirstOrDefaultAsync().AwaitBackground();

			if(value == null) {
				return null;
			}

			value.Timestamp = DateTime.SpecifyKind(value.Timestamp, DateTimeKind.Utc);

			return value;
		}
	}
}