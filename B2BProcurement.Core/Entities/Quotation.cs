using B2BProcurement.Core.Enums;

namespace B2BProcurement.Core.Entities
{
    /// <summary>
    /// Teklif entity'si.
    /// Tedarikçilerin RFQ'lara verdiği teklifleri tanımlar.
    /// </summary>
    public class Quotation : BaseEntity
    {
        /// <summary>
        /// Teklif numarası (otomatik oluşturulur).
        /// </summary>
        public string QuotationNumber { get; set; } = string.Empty;

        /// <summary>
        /// RFQ ID (Foreign Key).
        /// </summary>
        public int RfqId { get; set; }

        /// <summary>
        /// Tedarikçi şirket ID (Foreign Key).
        /// </summary>
        public int SupplierCompanyId { get; set; }

        /// <summary>
        /// Teklif durumu.
        /// </summary>
        public QuotationStatus Status { get; set; }

        /// <summary>
        /// Toplam teklif tutarı.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Teklif geçerlilik tarihi.
        /// </summary>
        public DateTime? ValidUntil { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Teklifin verildiği RFQ.
        /// </summary>
        public virtual RFQ? RFQ { get; set; }

        /// <summary>
        /// Teklifi veren şirket.
        /// </summary>
        public virtual Company? SupplierCompany { get; set; }

        /// <summary>
        /// Teklif kalemleri.
        /// </summary>
        public virtual ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();

        /// <summary>
        /// Teklif dokümanları.
        /// </summary>
        public virtual ICollection<QuotationDocument> Documents { get; set; } = new List<QuotationDocument>();

        #endregion
    }
}
