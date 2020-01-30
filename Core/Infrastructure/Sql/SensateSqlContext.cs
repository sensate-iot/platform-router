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
		public DbSet<Trigger> Triggers { get; set; }
		public DbSet<TriggerAction> TriggerActions { get; set; }
		public DbSet<TriggerInvocation> TriggerInvocations { get; set; }
		public DbSet<Blob> Blobs { get; set; }

		public SensateSqlContext(DbContextOptions<SensateSqlContext> options) : base(options)
		{ }

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
			builder.Entity<Trigger>().ToTable("Triggers");
			builder.Entity<TriggerAction>().ToTable("TriggerActions");
			builder.Entity<TriggerInvocation>().ToTable("TriggerInvocations");
			builder.Entity<Blob>().ToTable("Blobs");

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

			builder.Entity<Trigger>().Property(trigger => trigger.Id).UseIdentityByDefaultColumn();
			builder.Entity<Trigger>().HasIndex(trigger => trigger.SensorId);
			builder.Entity<Trigger>().HasMany(trigger => trigger.Invocations).WithOne()
				.HasForeignKey(invoc => invoc.TriggerId);
			builder.Entity<Trigger>().HasMany(trigger => trigger.Actions).WithOne()
				.HasForeignKey(action => action.TriggerId);

			builder.Entity<TriggerInvocation>().Property(invocation => invocation.Id).UseIdentityByDefaultColumn();
			builder.Entity<TriggerInvocation>().HasIndex(invocation => invocation.TriggerId);
			builder.Entity<TriggerInvocation>().HasAlternateKey(invocation =>
				new {invocation.MeasurementBucketId, invocation.MeasurementId, invocation.TriggerId});
			builder.Entity<TriggerAction>(action => {
				action.HasKey(t => new {t.TriggerId, t.Channel});
			});

			builder.Entity<Blob>().Property(blob => blob.Id).UseIdentityByDefaultColumn();
			builder.Entity<Blob>().HasIndex(blob => blob.SensorId);
			builder.Entity<Blob>().HasIndex(blob => new { blob.SensorId, blob.FileName }).IsUnique();
		}
	}
}
