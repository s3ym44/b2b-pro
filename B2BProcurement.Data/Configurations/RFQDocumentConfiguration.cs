using B2BProcurement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BProcurement.Data.Configurations
{
    /// <summary>
    /// RFQDocument entity için EF Core konfigürasyonu.
    /// </summary>
    public class RFQDocumentConfiguration : IEntityTypeConfiguration<RFQDocument>
    {
        public void Configure(EntityTypeBuilder<RFQDocument> builder)
        {
            builder.ToTable("RFQDocuments");

            builder.HasKey(r => r.Id);

            // Soft delete filter
            builder.HasQueryFilter(r => r.IsActive);

            // Property konfigürasyonları
            builder.Property(r => r.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(r => r.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            // İlişkiler
            builder.HasOne(r => r.RFQ)
                .WithMany(rfq => rfq.Documents)
                .HasForeignKey(r => r.RfqId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
