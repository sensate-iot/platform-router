/*
 * Change email token repository implementation.
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
	public class ChangeEmailTokenRepository : AbstractSqlRepository<ChangeEmailToken>, IChangeEmailTokenRepository
	{
		private readonly Random _rng;
		private const int UserTokenLength = 12;

		public ChangeEmailTokenRepository(SensateSqlContext context) : base(context)
		{
			this._rng = new Random(StaticRandom.Next());
		}

		public string Create(string token, string email)
		{
			var t = new ChangeEmailToken() {
				IdentityToken = token,
				UserToken = this._rng.NextString(ChangeEmailTokenRepository.UserTokenLength),
				Email = email
			};

			try {
				this.Create(t);
			} catch(Exception ex) {
				Debug.WriteLine($"Unable to create email reset token: {ex.Message}");
				return null;
			}

			return t.UserToken;
		}

		public ChangeEmailToken GetById(string id)
		{
			return this.Data.FirstOrDefault(x => x.UserToken == id);
		}
	}
}
