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
	public class TriggerContext : DbContext
	{
		public TriggerContext()
		{
		}

		public TriggerContext(DbContextOptions<TriggerContext> options)
			: base(options)
		{
		}

		public virtual DbSet<TriggerAction> TriggerActions { get; set; }
		public virtual DbSet<TriggerInvocation> TriggerInvocations { get; set; }
		public virtual DbSet<Trigger> Triggers { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasPostgresExtension("uuid-ossp");

			modelBuilder.Entity<TriggerAction>(entity => {
				entity.HasIndex(e => new {e.TriggerID, e.Channel, e.Target})
					.HasName("Alternative_Key_TriggerActions")
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
					.HasConstraintName("FK_TriggerActions_Triggers_TriggerID");
			});
			
			modelBuilder.Entity<TriggerInvocation>(entity =>
            {
                entity.HasIndex(e => e.TriggerActionID)
                    .HasName("IX_TriggerInvocations_TriggerActionID");

                entity.Property(e => e.ID)
                    .HasColumnName("ID")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.TriggerActionID).HasColumnName("TriggerActionID");

                entity.HasOne(d => d.TriggerAction)
                    .WithMany(p => p.TriggerInvocations)
                    .HasForeignKey(d => d.TriggerActionID);
            });

			modelBuilder.Entity<Trigger>(entity => {
				entity.HasIndex(e => e.SensorID)
					.HasName("IX_Triggers_SensorID");

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
		}
	}
}
