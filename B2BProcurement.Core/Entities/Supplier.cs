namespace B2BProcurement.Core.Entities
{
    /// <summary>
    /// Tedarikçi entity'si.
    /// Şirketlerin tedarikçi listesini tanımlar.
    /// </summary>
    public class Supplier : BaseEntity
    {
        /// <summary>
        /// Ana şirket ID (Foreign Key) - Tedarikçiyi ekleyen şirket.
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Tedarikçi şirket ID (Foreign Key) - Sistemde kayıtlı ise.
        /// </summary>
        public int? SupplierCompanyId { get; set; }

        /// <summary>
        /// Tedarikçi adı.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Vergi numarası.
        /// </summary>
        public string? TaxNumber { get; set; }

        /// <summary>
        /// E-posta adresi.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Telefon numarası.
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Sektör ID (Foreign Key).
        /// </summary>
        public int SectorId { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Tedarikçiyi ekleyen şirket.
        /// </summary>
        public virtual Company? Company { get; set; }

        /// <summary>
        /// Tedarikçinin sistemdeki şirket kaydı (varsa).
        /// </summary>
        public virtual Company? SupplierCompany { get; set; }

        /// <summary>
        /// Tedarikçinin ait olduğu sektör.
        /// </summary>
        public virtual Sector? Sector { get; set; }

        #endregion
    }
}
