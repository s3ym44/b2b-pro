using B2BProcurement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BProcurement.Data.Configurations
{
    /// <summary>
    /// RFQContact entity için EF Core konfigürasyonu.
    /// </summary>
    public class RFQContactConfiguration : IEntityTypeConfiguration<RFQContact>
    {
        public void Configure(EntityTypeBuilder<RFQContact> builder)
        {
            builder.ToTable("RFQContacts");

            builder.HasKey(r => r.Id);

            // Soft delete filter
            builder.HasQueryFilter(r => r.IsActive);

            // Property konfigürasyonları
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.Phone)
                .HasMaxLength(20);

            // İlişkiler
            builder.HasOne(r => r.RFQ)
                .WithMany(rfq => rfq.Contacts)
                .HasForeignKey(r => r.RfqId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
