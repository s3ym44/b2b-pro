using B2BProcurement.Core.Entities;
using B2BProcurement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace B2BProcurement.Data.Context
{
    /// <summary>
    /// Uygulama veritabanı bağlamı.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// ApplicationDbContext yapıcı metodu.
        /// </summary>
        /// <param name="options">DbContext seçenekleri.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        #region DbSets - Temel Tablolar

        /// <summary>
        /// Sektörler tablosu.
        /// </summary>
        public DbSet<Sector> Sectors { get; set; }

        /// <summary>
        /// Paketler tablosu.
        /// </summary>
        public DbSet<Package> Packages { get; set; }

        /// <summary>
        /// Şirketler tablosu.
        /// </summary>
        public DbSet<Company> Companies { get; set; }

        /// <summary>
        /// Kullanıcılar tablosu.
        /// </summary>
        public DbSet<User> Users { get; set; }

        #endregion

        #region DbSets - Tedarikçi & Malzeme

        /// <summary>
        /// Tedarikçiler tablosu.
        /// </summary>
        public DbSet<Supplier> Suppliers { get; set; }

        /// <summary>
        /// Malzemeler tablosu.
        /// </summary>
        public DbSet<Material> Materials { get; set; }

        /// <summary>
        /// Malzeme dokümanları tablosu.
        /// </summary>
        public DbSet<MaterialDocument> MaterialDocuments { get; set; }

        #endregion

        #region DbSets - RFQ (Teklif Talebi)

        /// <summary>
        /// RFQ'lar tablosu.
        /// </summary>
        public DbSet<RFQ> RFQs { get; set; }

        /// <summary>
        /// RFQ kalemleri tablosu.
        /// </summary>
        public DbSet<RFQItem> RFQItems { get; set; }

        /// <summary>
        /// RFQ dokümanları tablosu.
        /// </summary>
        public DbSet<RFQDocument> RFQDocuments { get; set; }

        /// <summary>
        /// RFQ iletişim kişileri tablosu.
        /// </summary>
        public DbSet<RFQContact> RFQContacts { get; set; }

        #endregion

        #region DbSets - Teklif

        /// <summary>
        /// Teklifler tablosu.
        /// </summary>
        public DbSet<Quotation> Quotations { get; set; }

        /// <summary>
        /// Teklif kalemleri tablosu.
        /// </summary>
        public DbSet<QuotationItem> QuotationItems { get; set; }

        /// <summary>
        /// Teklif dokümanları tablosu.
        /// </summary>
        public DbSet<QuotationDocument> QuotationDocuments { get; set; }

        #endregion

        #region DbSets - Sistem

        /// <summary>
        /// Bildirimler tablosu.
        /// </summary>
        public DbSet<Notification> Notifications { get; set; }

        /// <summary>
        /// Denetim logları tablosu.
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        #endregion

        #region SaveChanges Override

        /// <summary>
        /// Değişiklikleri kaydederken CreatedAt ve UpdatedAt alanlarını otomatik günceller.
        /// </summary>
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        /// <summary>
        /// Değişiklikleri asenkron kaydederken CreatedAt ve UpdatedAt alanlarını otomatik günceller.
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// IAuditableEntity implement eden entity'lerin tarih alanlarını günceller.
        /// </summary>
        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IAuditableEntity && 
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (IAuditableEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.Now;
                }

                entity.UpdatedAt = DateTime.Now;
            }
        }

        #endregion

        /// <summary>
        /// Model oluşturulurken çağrılır.
        /// Tüm konfigürasyonlar Configurations klasöründeki dosyalardan otomatik yüklenir.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurations klasöründeki tüm IEntityTypeConfiguration sınıflarını otomatik uygula
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
