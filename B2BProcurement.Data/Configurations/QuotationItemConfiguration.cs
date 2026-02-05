using B2BProcurement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BProcurement.Data.Configurations
{
    /// <summary>
    /// QuotationItem entity için EF Core konfigürasyonu.
    /// </summary>
    public class QuotationItemConfiguration : IEntityTypeConfiguration<QuotationItem>
    {
        public void Configure(EntityTypeBuilder<QuotationItem> builder)
        {
            builder.ToTable("QuotationItems");

            builder.HasKey(q => q.Id);

            // Soft delete filter
            builder.HasQueryFilter(q => q.IsActive);

            // Decimal precision - Quantity (18,3)
            builder.Property(q => q.OfferedQuantity)
                .HasPrecision(18, 3);

            builder.Property(q => q.ApprovedQuantity)
                .HasPrecision(18, 3);

            // Decimal precision - UnitPrice (18,4)
            builder.Property(q => q.UnitPrice)
                .HasPrecision(18, 4);

            // Decimal precision - TotalPrice (18,2)
            builder.Property(q => q.TotalPrice)
                .HasPrecision(18, 2);

            // İlişkiler
            builder.HasOne(q => q.Quotation)
                .WithMany(quot => quot.Items)
                .HasForeignKey(q => q.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(q => q.RFQItem)
                .WithMany(ri => ri.QuotationItems)
                .HasForeignKey(q => q.RfqItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
