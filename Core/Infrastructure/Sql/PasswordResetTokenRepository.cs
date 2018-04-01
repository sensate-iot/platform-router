/*
 * Password reset token repository.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Linq;
using System.Threading.Tasks;

using SensateService.Helpers;
using SensateService.Exceptions;
using SensateService.Infrastructure.Repositories;
using SensateService.Models;
using System.Diagnostics;

namespace SensateService.Infrastructure.Sql
{
	public class PasswordResetTokenRepository : AbstractSqlRepository<string, PasswordResetToken>, IPasswordResetTokenRepository
	{
		private Random _rng;

		public const int UserTokenLength = 12;

		public PasswordResetTokenRepository(SensateSqlContext context) : base(context)
		{
			this._rng = new Random();
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

		public override void Create(PasswordResetToken obj)
		{
			this.Data.Add(obj);
			this.Commit();
		}

		public async override Task CreateAsync(PasswordResetToken obj)
		{
			await this.Data.AddAsync(obj);
			await this.CommitAsync();
		}

		public override void Delete(string id)
		{
			throw new NotAllowedException("Cannot delete password reset token!");
		}

		public override Task DeleteAsync(string id)
		{
			throw new NotAllowedException("Cannot delete password reset token!");
		}

		public override PasswordResetToken GetById(string id)
		{
			return this.Data.FirstOrDefault(x => x.UserToken == id);
		}

		public override void Update(PasswordResetToken obj)
		{
			throw new NotAllowedException("Cannot update password reset token!");
		}
	}
}
