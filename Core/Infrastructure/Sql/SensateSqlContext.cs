/*
 * Sensate SQL database context.
 *
 * @author: Michel Megens
 * @email:  michel.megens@sonatolabs.com
 */

using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using SensateService.Models;

namespace SensateService.Infrastructure.Sql
{
	public class SensateSqlContext : IdentityDbContext<SensateUser, SensateRole, string,
		IdentityUserClaim<string>, SensateUserRole, IdentityUserLogin<string>,
		IdentityRoleClaim<string>, IdentityUserToken<string>>, IDataProtectionKeyContext
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
		public DbSet<SensorLink> SensorLinks { get; set; }
		public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

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
			builder.Entity<SensorLink>().ToTable("SensorLinks");
			builder.Entity<DataProtectionKey>().ToTable("DataProtectionKeys");

			builder.Entity<PasswordResetToken>().HasKey(k => k.UserToken);
			builder.Entity<AuthUserToken>(token => {
				token.HasKey(k => new {k.UserId, k.Value});
				token.HasIndex(x => x.UserId);
				token.HasIndex(x => x.Value);
			});

			builder.Entity<ChangePhoneNumberToken>(token => {
				token.HasKey(k => new {
					k.IdentityToken,
					k.PhoneNumber
				});

				token.HasAlternateKey(e => e.UserToken);

				token.HasIndex(x => x.PhoneNumber);

				token.HasOne<SensateUser>()
					.WithMany()
					.HasForeignKey(t => t.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
			});

			builder.Entity<SensateUser>(x => { x.HasIndex(u => u.BillingLockout); });

			builder.Entity<SensorLink>(link => {
				link.HasKey(k => new { k.UserId, k.SensorId });
				link.HasIndex(k => k.UserId);
				link.HasOne<SensateUser>().WithMany().HasForeignKey(x => x.UserId)
					.IsRequired().OnDelete(DeleteBehavior.Cascade);
			});

			builder.Entity<SensateApiKey>(key => {
				key.HasIndex(u => u.ApiKey).IsUnique();
				key.HasIndex(u => u.Type);
				key.HasOne(k => k.User).WithMany(user => user.ApiKeys)
					.HasForeignKey(k => k.UserId)
					.IsRequired();
			});

			/* Define User => UserRole relationships */
			builder.Entity<SensateUserRole>(userrole => {
				userrole.HasKey(role => new { role.UserId, role.RoleId });
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
			builder.Entity<AuditLog>().HasOne<SensateUser>().WithMany().HasForeignKey(log => log.AuthorId)
				.OnDelete(DeleteBehavior.Cascade);
			builder.Entity<AuditLog>().HasIndex(log => log.Method);
			builder.Entity<AuditLog>().HasIndex(log => log.AuthorId);

			builder.Entity<Trigger>().HasIndex(trigger => trigger.Type);
			builder.Entity<Trigger>().Property(trigger => trigger.Id).UseIdentityByDefaultColumn();
			builder.Entity<Trigger>().HasIndex(trigger => trigger.SensorId);
			builder.Entity<Trigger>().HasMany(trigger => trigger.Invocations).WithOne()
				.HasForeignKey(invoc => invoc.TriggerId).OnDelete(DeleteBehavior.Cascade);
			builder.Entity<Trigger>().HasMany(trigger => trigger.Actions).WithOne()
				.HasForeignKey(action => action.TriggerId).OnDelete(DeleteBehavior.Cascade);

			builder.Entity<TriggerInvocation>().Property(invocation => invocation.Id).UseIdentityByDefaultColumn();
			builder.Entity<TriggerInvocation>().HasIndex(invocation => invocation.TriggerId);
			builder.Entity<TriggerAction>(action => {
				action.HasKey(t => new { t.TriggerId, t.Channel });
			});

			builder.Entity<Blob>().Property(blob => blob.Id).UseIdentityByDefaultColumn();
			builder.Entity<Blob>().HasIndex(blob => blob.SensorId);
			builder.Entity<Blob>().HasIndex(blob => new { blob.SensorId, blob.FileName }).IsUnique();
		}
	}
}
