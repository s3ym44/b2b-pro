using B2BProcurement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BProcurement.Data.Configurations
{
    /// <summary>
    /// Supplier entity için EF Core konfigürasyonu.
    /// </summary>
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ToTable("Suppliers");

            builder.HasKey(s => s.Id);

            // Soft delete filter
            builder.HasQueryFilter(s => s.IsActive);

            // Property konfigürasyonları
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.TaxNumber)
                .HasMaxLength(20);

            builder.Property(s => s.Email)
                .HasMaxLength(100);

            builder.Property(s => s.Phone)
                .HasMaxLength(20);

            // İlişkiler
            builder.HasOne(s => s.Company)
                .WithMany(c => c.Suppliers)
                .HasForeignKey(s => s.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.SupplierCompany)
                .WithMany()
                .HasForeignKey(s => s.SupplierCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Sector)
                .WithMany(sec => sec.Suppliers)
                .HasForeignKey(s => s.SectorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
