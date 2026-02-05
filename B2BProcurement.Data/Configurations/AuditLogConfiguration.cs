using B2BProcurement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BProcurement.Data.Configurations
{
    /// <summary>
    /// AuditLog entity için EF Core konfigürasyonu.
    /// </summary>
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(a => a.Id);

            // Property konfigürasyonları
            builder.Property(a => a.Action)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.OldValues)
                .HasMaxLength(4000);

            builder.Property(a => a.NewValues)
                .HasMaxLength(4000);

            builder.Property(a => a.IpAddress)
                .HasMaxLength(50);

            // Index
            builder.HasIndex(a => a.EntityType);
            builder.HasIndex(a => a.CreatedAt);
        }
    }
}
