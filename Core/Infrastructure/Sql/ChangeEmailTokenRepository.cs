/*
 * Change email token repository implementation.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Linq;
using System.Diagnostics;

using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class ChangeEmailTokenRepository : AbstractSqlRepository<ChangeEmailToken>, IChangeEmailTokenRepository
	{
		private Random _rng;
		private const int UserTokenLength = 12;

		public ChangeEmailTokenRepository(SensateSqlContext context) : base(context)
		{
			this._rng = new Random();
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
