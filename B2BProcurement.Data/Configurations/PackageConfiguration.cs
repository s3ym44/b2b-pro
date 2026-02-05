using B2BProcurement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BProcurement.Data.Configurations
{
    /// <summary>
    /// Package entity için EF Core konfigürasyonu.
    /// </summary>
    public class PackageConfiguration : IEntityTypeConfiguration<Package>
    {
        public void Configure(EntityTypeBuilder<Package> builder)
        {
            builder.ToTable("Packages");

            builder.HasKey(p => p.Id);

            // Soft delete filter
            builder.HasQueryFilter(p => p.IsActive);

            // Property konfigürasyonları
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Decimal precision - Price (18,2)
            builder.Property(p => p.Price)
                .HasPrecision(18, 2);
        }
    }
}
