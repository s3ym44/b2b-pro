using B2BProcurement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BProcurement.Data.Configurations
{
    /// <summary>
    /// RFQItem entity için EF Core konfigürasyonu.
    /// </summary>
    public class RFQItemConfiguration : IEntityTypeConfiguration<RFQItem>
    {
        public void Configure(EntityTypeBuilder<RFQItem> builder)
        {
            builder.ToTable("RFQItems");

            builder.HasKey(r => r.Id);

            // Soft delete filter
            builder.HasQueryFilter(r => r.IsActive);

            // Property konfigürasyonları
            builder.Property(r => r.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(r => r.Unit)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(r => r.TechnicalSpecs)
                .HasMaxLength(4000);

            // Decimal precision - Quantity (18,3)
            builder.Property(r => r.Quantity)
                .HasPrecision(18, 3);

            // İlişkiler
            builder.HasOne(r => r.RFQ)
                .WithMany(rfq => rfq.Items)
                .HasForeignKey(r => r.RfqId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Material)
                .WithMany(m => m.RFQItems)
                .HasForeignKey(r => r.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
