using B2BProcurement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BProcurement.Data.Configurations
{
    /// <summary>
    /// QuotationDocument entity için EF Core konfigürasyonu.
    /// </summary>
    public class QuotationDocumentConfiguration : IEntityTypeConfiguration<QuotationDocument>
    {
        public void Configure(EntityTypeBuilder<QuotationDocument> builder)
        {
            builder.ToTable("QuotationDocuments");

            builder.HasKey(q => q.Id);

            // Soft delete filter
            builder.HasQueryFilter(q => q.IsActive);

            // Property konfigürasyonları
            builder.Property(q => q.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(q => q.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            // İlişkiler
            builder.HasOne(q => q.Quotation)
                .WithMany(quot => quot.Documents)
                .HasForeignKey(q => q.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
