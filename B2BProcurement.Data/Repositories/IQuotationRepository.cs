using B2BProcurement.Core.Entities;
using B2BProcurement.Core.Enums;

namespace B2BProcurement.Data.Repositories
{
    /// <summary>
    /// Quotation için özelleştirilmiş repository arayüzü.
    /// </summary>
    public interface IQuotationRepository : IRepository<Quotation>
    {
        /// <summary>
        /// Teklifi ilişkili kalemler ve dokümanlarla birlikte getirir.
        /// </summary>
        /// <param name="id">Teklif kimliği.</param>
        /// <returns>İlişkili verilerle birlikte teklif.</returns>
        Task<Quotation?> GetWithItemsAsync(int id);

        /// <summary>
        /// RFQ'ya ait teklifleri getirir.
        /// </summary>
        /// <param name="rfqId">RFQ kimliği.</param>
        /// <returns>RFQ'ya ait teklif listesi.</returns>
        Task<IEnumerable<Quotation>> GetByRfqAsync(int rfqId);

        /// <summary>
        /// Tedarikçi şirkete ait teklifleri getirir.
        /// </summary>
        /// <param name="supplierCompanyId">Tedarikçi şirket kimliği.</param>
        /// <returns>Tedarikçiye ait teklif listesi.</returns>
        Task<IEnumerable<Quotation>> GetBySupplierAsync(int supplierCompanyId);

        /// <summary>
        /// RFQ için teklif karşılaştırma matrisi verisini getirir.
        /// Her bir RFQ kalemi için tüm tekliflerin fiyat ve miktar bilgilerini içerir.
        /// </summary>
        /// <param name="rfqId">RFQ kimliği.</param>
        /// <returns>Karşılaştırma matrisi verisi.</returns>
        Task<IEnumerable<QuotationComparisonItem>> GetComparisonMatrixAsync(int rfqId);

        /// <summary>
        /// Belirli duruma sahip teklifleri getirir.
        /// </summary>
        /// <param name="status">Teklif durumu.</param>
        /// <returns>İlgili duruma sahip teklif listesi.</returns>
        Task<IEnumerable<Quotation>> GetByStatusAsync(QuotationStatus status);
    }

    /// <summary>
    /// Teklif karşılaştırma matrisi için veri modeli.
    /// </summary>
    public class QuotationComparisonItem
    {
        /// <summary>
        /// RFQ kalemi bilgisi.
        /// </summary>
        public RFQItem RfqItem { get; set; } = null!;

        /// <summary>
        /// Bu kaleme ait tüm teklif kalemleri.
        /// </summary>
        public List<QuotationItemDetail> QuotationItems { get; set; } = new();
    }

    /// <summary>
    /// Teklif kalemi detay bilgisi.
    /// </summary>
    public class QuotationItemDetail
    {
        /// <summary>
        /// Teklif veren şirket adı.
        /// </summary>
        public string SupplierName { get; set; } = string.Empty;

        /// <summary>
        /// Teklif numarası.
        /// </summary>
        public string QuotationNumber { get; set; } = string.Empty;

        /// <summary>
        /// Birim fiyat.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Teklif edilen miktar.
        /// </summary>
        public decimal OfferedQuantity { get; set; }

        /// <summary>
        /// Toplam fiyat.
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
    }
}
