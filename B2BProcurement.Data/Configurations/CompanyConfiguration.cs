using B2BProcurement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BProcurement.Data.Configurations
{
    /// <summary>
    /// Company entity için EF Core konfigürasyonu.
    /// </summary>
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Companies");

            builder.HasKey(c => c.Id);

            // Soft delete filter
            builder.HasQueryFilter(c => c.IsActive);

            // Property konfigürasyonları
            builder.Property(c => c.CompanyName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.TaxNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.TaxOffice)
                .HasMaxLength(100);

            builder.Property(c => c.Address)
                .HasMaxLength(500);

            builder.Property(c => c.City)
                .HasMaxLength(100);

            builder.Property(c => c.Phone)
                .HasMaxLength(20);

            builder.Property(c => c.Email)
                .HasMaxLength(100);

            // Unique index
            builder.HasIndex(c => c.TaxNumber)
                .IsUnique();

            // İlişkiler
            builder.HasOne(c => c.Sector)
                .WithMany(s => s.Companies)
                .HasForeignKey(c => c.SectorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Package)
                .WithMany(p => p.Companies)
                .HasForeignKey(c => c.PackageId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
