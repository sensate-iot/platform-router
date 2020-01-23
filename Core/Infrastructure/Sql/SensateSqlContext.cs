/*
 * Sensate SQL database context.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
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
		public new DbSet<AuthUserToken> UserTokens { get; set; }
		public DbSet<ChangePhoneNumberToken> ChangePhoneNumberTokens { get; set; }
		public DbSet<SensateApiKey> ApiKeys { get; set; }
		public DbSet<AuditLog> AuditLogs { get; set; }

		public SensateSqlContext(DbContextOptions<SensateSqlContext> options) : base(options)
		{}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
			builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
			builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
			builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
			builder.Entity<SensateUser>().ToTable("Users");
			builder.Entity<SensateRole>().ToTable("Roles");
			builder.Entity<SensateUserRole>().ToTable("UserRoles");

			builder.Entity<PasswordResetToken>().HasKey( k => k.UserToken );
			builder.Entity<AuthUserToken>().HasKey(k => new { k.UserId, k.Value });

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

			/* Define User => UserRole relationships */
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

			builder.Entity<AuditLog>().Property(log => log.Id).UseIdentityByDefaultColumn();
			builder.Entity<AuditLog>().HasOne<SensateUser>().WithMany().HasForeignKey(log => log.AuthorId);
			builder.Entity<AuditLog>().HasIndex(log => log.Method);

			/*builder.HasSequence<long>("Id_sequence")
				.StartsAt(1)
				.IncrementsBy(1);
			builder.Entity<AuditLog>()
				.Property(o => o.Id)
				.HasDefaultValueSql("nextval('\"Id_sequence\"')");*/
		}
	}
}
