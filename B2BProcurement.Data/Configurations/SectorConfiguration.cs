using B2BProcurement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BProcurement.Data.Configurations
{
    /// <summary>
    /// Sector entity için EF Core konfigürasyonu.
    /// </summary>
    public class SectorConfiguration : IEntityTypeConfiguration<Sector>
    {
        public void Configure(EntityTypeBuilder<Sector> builder)
        {
            builder.ToTable("Sectors");

            builder.HasKey(s => s.Id);

            // Soft delete filter
            builder.HasQueryFilter(s => s.IsActive);

            // Property konfigürasyonları
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.NameEn)
                .HasMaxLength(200);

            builder.Property(s => s.Code)
                .HasMaxLength(50);

            builder.Property(s => s.Description)
                .HasMaxLength(1000);

            // Index
            builder.HasIndex(s => s.Code);
        }
    }
}
