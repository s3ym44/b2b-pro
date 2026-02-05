using B2BProcurement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BProcurement.Data.Configurations
{
    /// <summary>
    /// RFQ entity için EF Core konfigürasyonu.
    /// </summary>
    public class RFQConfiguration : IEntityTypeConfiguration<RFQ>
    {
        public void Configure(EntityTypeBuilder<RFQ> builder)
        {
            builder.ToTable("RFQs");

            builder.HasKey(r => r.Id);

            // Soft delete filter
            builder.HasQueryFilter(r => r.IsActive);

            // Property konfigürasyonları
            builder.Property(r => r.RfqNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(r => r.Title)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(r => r.Currency)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("TRY");

            // Unique index
            builder.HasIndex(r => r.RfqNumber)
                .IsUnique();

            // İlişkiler
            builder.HasOne(r => r.Company)
                .WithMany(c => c.RFQs)
                .HasForeignKey(r => r.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Sector)
                .WithMany(s => s.RFQs)
                .HasForeignKey(r => r.SectorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
