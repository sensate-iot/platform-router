/*
 * Sensate SQL database context.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class SensateSqlContext : IdentityDbContext<SensateUser, SensateRole, string,
		IdentityUserClaim<string>, SensateUserRole, IdentityUserLogin<string>,
		IdentityRoleClaim<string>, IdentityUserToken<string>>
	{
		public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
		public DbSet<ChangeEmailToken> ChangeEmailTokens { get; set; }
		public new DbSet<UserToken> UserTokens { get; set; }
		public DbSet<ChangePhoneNumberToken> ChangePhoneNumberTokens { get; set; }
		public DbSet<SensateApiKey> ApiKeys { get; set; }

		public SensateSqlContext(DbContextOptions<SensateSqlContext> options) :
			base(options)
		{}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<PasswordResetToken>().HasKey(
				k => k.UserToken
			);

			builder.Entity<UserToken>().HasKey(k => new {
				k.UserId, k.Value
			});

			builder.Entity<ChangePhoneNumberToken>().HasAlternateKey(e => e.UserToken)
				.HasName("AlternateKey_UserToken");
			builder.Entity<ChangePhoneNumberToken>().HasKey(k => new {
				k.IdentityToken, k.PhoneNumber
			});

			builder.Entity<SensateApiKey>(key => {
				key.HasIndex(u => u.ApiKey).IsUnique();
				key.HasOne(k => k.User).WithMany(user => user.ApiKeys)
					.HasForeignKey(k => k.UserId)
					.IsRequired();
			});

			builder.Entity<SensateUserRole>(userrole => {
				userrole.HasKey(role => new {role.UserId, role.RoleId});
				userrole.HasOne(role => role.Role)
					.WithMany(role => role.UserRoles)
					.HasForeignKey(role => role.RoleId)
					.IsRequired();

				userrole.HasOne(role => role.User)
					.WithMany(user => user.UserRoles)
					.HasForeignKey(user => user.UserId)
					.IsRequired();
			});
		}
	}
}
