using B2BProcurement.Core.Enums;

namespace B2BProcurement.Core.Entities
{
    /// <summary>
    /// Teklif kalemi entity'si.
    /// Tekliflerdeki her bir kalem için fiyat ve miktar bilgilerini tanımlar.
    /// </summary>
    public class QuotationItem : BaseEntity
    {
        /// <summary>
        /// Teklif ID (Foreign Key).
        /// </summary>
        public int QuotationId { get; set; }

        /// <summary>
        /// RFQ Kalemi ID (Foreign Key).
        /// </summary>
        public int RfqItemId { get; set; }

        /// <summary>
        /// Teklif edilen miktar.
        /// </summary>
        public decimal OfferedQuantity { get; set; }

        /// <summary>
        /// Birim fiyat.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Toplam fiyat (OfferedQuantity * UnitPrice).
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Teslim tarihi.
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// Onay durumu.
        /// </summary>
        public ApprovalStatus ApprovalStatus { get; set; }

        /// <summary>
        /// Onaylanan miktar.
        /// </summary>
        public decimal? ApprovedQuantity { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Kalemin ait olduğu teklif.
        /// </summary>
        public virtual Quotation? Quotation { get; set; }

        /// <summary>
        /// İlişkili RFQ kalemi.
        /// </summary>
        public virtual RFQItem? RFQItem { get; set; }

        #endregion
    }
}
