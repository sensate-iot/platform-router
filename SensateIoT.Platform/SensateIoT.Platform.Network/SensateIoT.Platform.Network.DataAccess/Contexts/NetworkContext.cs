/*
 * Trigger database context.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using Microsoft.EntityFrameworkCore;

using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.DataAccess.Contexts
{
	public class NetworkContext : DbContext
	{
		public NetworkContext(DbContextOptions<NetworkContext> options)
			: base(options)
		{
		}

		public virtual DbSet<TriggerAction> TriggerActions { get; set; }
		public virtual DbSet<TriggerInvocation> TriggerInvocations { get; set; }
		public virtual DbSet<Trigger> Triggers { get; set; }
		public virtual DbSet<LiveDataHandler> LiveDataHandlers { get; set; }
		public virtual DbSet<SensorLink> SensorLinks { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.HasPostgresExtension("uuid-ossp");

			modelBuilder.Entity<TriggerAction>(entity => {
				entity.HasIndex(e => new { e.TriggerID, e.Channel, e.Target }, "Alternative_Key_TriggerActions")
					.IsUnique();

				entity.Property(e => e.ID)
					.HasColumnName("ID")
					.UseIdentityAlwaysColumn();

				entity.Property(e => e.Message).IsRequired();
				entity.Property(e => e.Target).HasMaxLength(255);
				entity.Property(e => e.TriggerID).HasColumnName("TriggerID");

				entity.HasOne(d => d.Trigger)
					.WithMany(p => p.TriggerActions)
					.HasForeignKey(d => d.TriggerID)
					.HasConstraintName("FK_TriggerActions_Triggers_TriggerId");
			});

			modelBuilder.Entity<TriggerInvocation>(entity => {
				entity.HasIndex(e => e.ActionID, "IX_TriggerInvocations_ActionID");

				entity.Property(e => e.ID)
					.HasColumnName("ID")
					.UseIdentityAlwaysColumn();

				entity.Property(e => e.ActionID).HasColumnName("ActionID");

				entity.HasOne(d => d.Action)
					.WithMany(p => p.TriggerInvocations)
					.HasForeignKey(d => d.ActionID);
			});

			modelBuilder.Entity<Trigger>(entity => {
				entity.HasIndex(e => e.SensorID)
					.HasDatabaseName("IX_Triggers_SensorID");

				entity.HasIndex(e => e.Type);

				entity.Property(e => e.ID)
					.HasColumnName("ID")
					.UseIdentityAlwaysColumn();

				entity.Property(e => e.KeyValue)
					.IsRequired()
					.HasMaxLength(32);

				entity.Property(e => e.LowerEdge).HasColumnType("numeric");

				entity.Property(e => e.SensorID)
					.IsRequired()
					.HasColumnName("SensorID")
					.HasMaxLength(24);

				entity.Property(e => e.UpperEdge).HasColumnType("numeric");
			});

			modelBuilder.Entity<LiveDataHandler>(entity => {
				entity.Property(e => e.ID)
					.HasColumnName("ID")
					.UseIdentityAlwaysColumn();

				entity.HasIndex(h => h.Enabled)
					.HasDatabaseName("IX_LiveDataHandlers_Enabled");
				entity.Property(h => h.Enabled)
					.IsRequired();

				entity.HasIndex(h => h.Name)
					.HasDatabaseName("IX_LiveDataHandlers_Name")
					.IsUnique();
			});

			modelBuilder.Entity<SensorLink>(link => {
				link.HasIndex(e => e.UserId).HasDatabaseName("IX_SensorLinks_UserId");
				link.HasIndex(e => e.UserId).HasDatabaseName("IX_SensorLinks_SensorId");
				link.HasKey(x => new { x.UserId, x.SensorId }).HasName("PK_SensorLinks");
			});
		}
	}
}
