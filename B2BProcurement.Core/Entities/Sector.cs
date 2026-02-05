namespace B2BProcurement.Core.Entities
{
    /// <summary>
    /// Sektör entity'si.
    /// Şirketlerin ve malzemelerin ait olduğu sektörleri tanımlar.
    /// </summary>
    public class Sector : BaseEntity
    {
        /// <summary>
        /// Sektör adı (Türkçe).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Sektör adı (İngilizce).
        /// </summary>
        public string? NameEn { get; set; }

        /// <summary>
        /// Sektör kodu.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Sektör açıklaması.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Görüntüleme sırası.
        /// </summary>
        public int DisplayOrder { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Bu sektöre ait şirketler.
        /// </summary>
        public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

        /// <summary>
        /// Bu sektöre ait malzemeler.
        /// </summary>
        public virtual ICollection<Material> Materials { get; set; } = new List<Material>();

        /// <summary>
        /// Bu sektöre ait tedarikçiler.
        /// </summary>
        public virtual ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();

        /// <summary>
        /// Bu sektöre ait RFQ'lar.
        /// </summary>
        public virtual ICollection<RFQ> RFQs { get; set; } = new List<RFQ>();

        #endregion
    }
}
