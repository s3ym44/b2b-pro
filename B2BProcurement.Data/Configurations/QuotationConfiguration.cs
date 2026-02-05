using B2BProcurement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BProcurement.Data.Configurations
{
    /// <summary>
    /// Quotation entity için EF Core konfigürasyonu.
    /// </summary>
    public class QuotationConfiguration : IEntityTypeConfiguration<Quotation>
    {
        public void Configure(EntityTypeBuilder<Quotation> builder)
        {
            builder.ToTable("Quotations");

            builder.HasKey(q => q.Id);

            // Soft delete filter
            builder.HasQueryFilter(q => q.IsActive);

            // Property konfigürasyonları
            builder.Property(q => q.QuotationNumber)
                .IsRequired()
                .HasMaxLength(50);

            // Decimal precision - TotalAmount (18,2)
            builder.Property(q => q.TotalAmount)
                .HasPrecision(18, 2);

            // Unique index
            builder.HasIndex(q => q.QuotationNumber)
                .IsUnique();

            // İlişkiler
            builder.HasOne(q => q.RFQ)
                .WithMany(r => r.Quotations)
                .HasForeignKey(q => q.RfqId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(q => q.SupplierCompany)
                .WithMany(c => c.Quotations)
                .HasForeignKey(q => q.SupplierCompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
