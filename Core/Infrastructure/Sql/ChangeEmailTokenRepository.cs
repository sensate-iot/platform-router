/*
 * Change email token repository implementation.
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
	public class ChangeEmailTokenRepository : AbstractSqlRepository<string, ChangeEmailToken>, IChangeEmailTokenRepository
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

		public override void Create(ChangeEmailToken obj)
		{
			this.Data.Add(obj);
			this.Commit();
		}

		public override async Task CreateAsync(ChangeEmailToken obj)
		{
			var tasks = new[] {
				this.Data.AddAsync(obj),
				this.CommitAsync()
			};

			await Task.WhenAll(tasks).AwaitSafely();
		}

		public override void Delete(string id)
		{
			throw new NotAllowedException("Cannot delete email reset token!");
		}

		public override Task DeleteAsync(string id)
		{
			throw new NotAllowedException("Cannot delete email reset token!");
		}

		public override ChangeEmailToken GetById(string id)
		{
			return this.Data.FirstOrDefault(x => x.UserToken == id);
		}

		public override void Update(ChangeEmailToken obj)
		{
			throw new NotAllowedException("Cannot update email reset token!");
		}
	}
}
