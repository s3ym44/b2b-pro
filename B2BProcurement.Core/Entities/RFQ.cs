using B2BProcurement.Core.Enums;

namespace B2BProcurement.Core.Entities
{
    /// <summary>
    /// RFQ (Request for Quotation) - Teklif Talebi entity'si.
    /// Şirketlerin tedarikçilerden teklif istediği talepleri tanımlar.
    /// </summary>
    public class RFQ : BaseEntity
    {
        /// <summary>
        /// RFQ numarası (otomatik oluşturulur).
        /// </summary>
        public string RfqNumber { get; set; } = string.Empty;

        /// <summary>
        /// Şirket ID (Foreign Key) - RFQ'yu oluşturan şirket.
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// RFQ başlığı.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Sektör ID (Foreign Key).
        /// </summary>
        public int SectorId { get; set; }

        /// <summary>
        /// RFQ durumu.
        /// </summary>
        public RfqStatus Status { get; set; }

        /// <summary>
        /// RFQ görünürlüğü.
        /// </summary>
        public RfqVisibility Visibility { get; set; }

        /// <summary>
        /// Teklif başlangıç tarihi.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Teklif bitiş tarihi.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Para birimi (TRY, USD, EUR vb.).
        /// </summary>
        public string Currency { get; set; } = "TRY";

        #region Navigation Properties

        /// <summary>
        /// RFQ'yu oluşturan şirket.
        /// </summary>
        public virtual Company? Company { get; set; }

        /// <summary>
        /// RFQ'nun ait olduğu sektör.
        /// </summary>
        public virtual Sector? Sector { get; set; }

        /// <summary>
        /// RFQ kalemleri.
        /// </summary>
        public virtual ICollection<RFQItem> Items { get; set; } = new List<RFQItem>();

        /// <summary>
        /// RFQ dokümanları.
        /// </summary>
        public virtual ICollection<RFQDocument> Documents { get; set; } = new List<RFQDocument>();

        /// <summary>
        /// RFQ iletişim kişileri.
        /// </summary>
        public virtual ICollection<RFQContact> Contacts { get; set; } = new List<RFQContact>();

        /// <summary>
        /// Bu RFQ'ya gelen teklifler.
        /// </summary>
        public virtual ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();

        #endregion
    }
}
