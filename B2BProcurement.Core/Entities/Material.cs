namespace B2BProcurement.Core.Entities
{
    /// <summary>
    /// Malzeme entity'si.
    /// Şirketlerin satın almak istediği malzeme/ürünleri tanımlar.
    /// </summary>
    public class Material : BaseEntity
    {
        /// <summary>
        /// Şirket ID (Foreign Key).
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Malzeme kodu.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Malzeme adı.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Malzeme açıklaması.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Ölçü birimi (adet, kg, lt, vb.).
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Sektör ID (Foreign Key).
        /// </summary>
        public int SectorId { get; set; }

        /// <summary>
        /// Herkese açık mı?
        /// </summary>
        public bool IsPublic { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Malzemenin ait olduğu şirket.
        /// </summary>
        public virtual Company? Company { get; set; }

        /// <summary>
        /// Malzemenin ait olduğu sektör.
        /// </summary>
        public virtual Sector? Sector { get; set; }

        /// <summary>
        /// Malzemeye ait dökümanlar.
        /// </summary>
        public virtual ICollection<MaterialDocument> Documents { get; set; } = new List<MaterialDocument>();

        /// <summary>
        /// Bu malzemenin kullanıldığı RFQ kalemleri.
        /// </summary>
        public virtual ICollection<RFQItem> RFQItems { get; set; } = new List<RFQItem>();

        #endregion
    }
}
