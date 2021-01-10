/*
 * Password reset token repository.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Diagnostics;
using System.Linq;
using SensateIoT.API.Common.Core.Helpers;
using SensateIoT.API.Common.Core.Infrastructure.Repositories;
using SensateIoT.API.Common.Data.Models;

namespace SensateIoT.API.Common.Core.Infrastructure.Sql
{
	public class PasswordResetTokenRepository : AbstractSqlRepository<PasswordResetToken>, IPasswordResetTokenRepository
	{
		private readonly Random _rng;
		private const int UserTokenLength = 12;

		public PasswordResetTokenRepository(SensateSqlContext context) : base(context)
		{
			this._rng = new Random(StaticRandom.Next());
		}

		public string Create(string token)
		{
			var t = new PasswordResetToken() {
				IdentityToken = token,
				UserToken = this._rng.NextString(PasswordResetTokenRepository.UserTokenLength)
			};

			try {
				this.Create(t);
			} catch(Exception ex) {
				Debug.WriteLine($"Unable to create password reset token: {ex.Message}");
				return null;
			}

			return t.UserToken;
		}

		public PasswordResetToken GetById(string id)
		{
			return this.Data.FirstOrDefault(x => x.UserToken == id);
		}
	}
}
