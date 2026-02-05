using B2BProcurement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BProcurement.Data.Configurations
{
    /// <summary>
    /// MaterialDocument entity için EF Core konfigürasyonu.
    /// </summary>
    public class MaterialDocumentConfiguration : IEntityTypeConfiguration<MaterialDocument>
    {
        public void Configure(EntityTypeBuilder<MaterialDocument> builder)
        {
            builder.ToTable("MaterialDocuments");

            builder.HasKey(m => m.Id);

            // Soft delete filter
            builder.HasQueryFilter(m => m.IsActive);

            // Property konfigürasyonları
            builder.Property(m => m.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(m => m.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(m => m.FileType)
                .HasMaxLength(50);

            // İlişkiler
            builder.HasOne(m => m.Material)
                .WithMany(mat => mat.Documents)
                .HasForeignKey(m => m.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
