using B2BProcurement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BProcurement.Data.Configurations
{
    /// <summary>
    /// Material entity için EF Core konfigürasyonu.
    /// </summary>
    public class MaterialConfiguration : IEntityTypeConfiguration<Material>
    {
        public void Configure(EntityTypeBuilder<Material> builder)
        {
            builder.ToTable("Materials");

            builder.HasKey(m => m.Id);

            // Soft delete filter
            builder.HasQueryFilter(m => m.IsActive);

            // Property konfigürasyonları
            builder.Property(m => m.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(m => m.Description)
                .HasMaxLength(1000);

            builder.Property(m => m.Unit)
                .IsRequired()
                .HasMaxLength(20);

            // Index for Code within Company (unique constraint)
            builder.HasIndex(m => new { m.CompanyId, m.Code })
                .IsUnique();

            // İlişkiler
            builder.HasOne(m => m.Company)
                .WithMany(c => c.Materials)
                .HasForeignKey(m => m.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Sector)
                .WithMany(s => s.Materials)
                .HasForeignKey(m => m.SectorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
