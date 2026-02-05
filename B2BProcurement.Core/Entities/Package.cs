namespace B2BProcurement.Core.Entities
{
    /// <summary>
    /// Paket entity'si.
    /// Şirketlerin abone olduğu hizmet paketlerini tanımlar.
    /// </summary>
    public class Package : BaseEntity
    {
        /// <summary>
        /// Paket adı.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Paket fiyatı (aylık).
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Maksimum kullanıcı sayısı.
        /// </summary>
        public int MaxUsers { get; set; }

        /// <summary>
        /// Maksimum malzeme sayısı.
        /// </summary>
        public int MaxMaterials { get; set; }

        /// <summary>
        /// Aylık maksimum RFQ sayısı.
        /// </summary>
        public int MaxRfqPerMonth { get; set; }

        /// <summary>
        /// SAP entegrasyonu kullanabilir mi?
        /// </summary>
        public bool CanUseSapIntegration { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Bu pakete abone şirketler.
        /// </summary>
        public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

        #endregion
    }
}
