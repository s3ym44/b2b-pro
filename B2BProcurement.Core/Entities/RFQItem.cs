namespace B2BProcurement.Core.Entities
{
    /// <summary>
    /// RFQ kalemi entity'si.
    /// RFQ'daki her bir malzeme/hizmet kalemini tanımlar.
    /// </summary>
    public class RFQItem : BaseEntity
    {
        /// <summary>
        /// RFQ ID (Foreign Key).
        /// </summary>
        public int RfqId { get; set; }

        /// <summary>
        /// Malzeme ID (Foreign Key) - Opsiyonel.
        /// </summary>
        public int? MaterialId { get; set; }

        /// <summary>
        /// Kalem açıklaması.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Talep edilen miktar.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Ölçü birimi.
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Teknik özellikler.
        /// </summary>
        public string? TechnicalSpecs { get; set; }

        /// <summary>
        /// İstenen teslim tarihi.
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Kalemin ait olduğu RFQ.
        /// </summary>
        public virtual RFQ? RFQ { get; set; }

        /// <summary>
        /// İlişkili malzeme (varsa).
        /// </summary>
        public virtual Material? Material { get; set; }

        /// <summary>
        /// Bu kaleme gelen teklif kalemleri.
        /// </summary>
        public virtual ICollection<QuotationItem> QuotationItems { get; set; } = new List<QuotationItem>();

        #endregion
    }
}
