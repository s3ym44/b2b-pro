namespace B2BProcurement.Core.Entities
{
    /// <summary>
    /// Şirket entity'si.
    /// Sistemdeki tüm şirketleri (alıcı ve tedarikçi) tanımlar.
    /// </summary>
    public class Company : BaseEntity
    {
        /// <summary>
        /// Şirket adı.
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Vergi numarası.
        /// </summary>
        public string TaxNumber { get; set; } = string.Empty;

        /// <summary>
        /// Vergi dairesi.
        /// </summary>
        public string? TaxOffice { get; set; }

        /// <summary>
        /// Adres.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Şehir.
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Telefon numarası.
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// E-posta adresi.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Sektör ID (Foreign Key).
        /// </summary>
        public int SectorId { get; set; }

        /// <summary>
        /// Paket ID (Foreign Key).
        /// </summary>
        public int PackageId { get; set; }

        /// <summary>
        /// Abonelik bitiş tarihi.
        /// </summary>
        public DateTime? SubscriptionEndDate { get; set; }

        /// <summary>
        /// SAP entegrasyonu aktif mi?
        /// </summary>
        public bool IsSapIntegrated { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Şirketin ait olduğu sektör.
        /// </summary>
        public virtual Sector? Sector { get; set; }

        /// <summary>
        /// Şirketin abone olduğu paket.
        /// </summary>
        public virtual Package? Package { get; set; }

        /// <summary>
        /// Şirketteki kullanıcılar.
        /// </summary>
        public virtual ICollection<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// Şirkete ait malzemeler.
        /// </summary>
        public virtual ICollection<Material> Materials { get; set; } = new List<Material>();

        /// <summary>
        /// Şirketin tedarikçileri.
        /// </summary>
        public virtual ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();

        /// <summary>
        /// Şirketin oluşturduğu RFQ'lar.
        /// </summary>
        public virtual ICollection<RFQ> RFQs { get; set; } = new List<RFQ>();

        /// <summary>
        /// Şirketin tedarikçi olarak verdiği teklifler.
        /// </summary>
        public virtual ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();

        #endregion
    }
}
