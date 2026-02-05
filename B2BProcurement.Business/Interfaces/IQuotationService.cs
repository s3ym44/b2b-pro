using B2BProcurement.Business.DTOs.Quotation;
using B2BProcurement.Core.Enums;

namespace B2BProcurement.Business.Interfaces
{
    /// <summary>
    /// Teklif servisi arayüzü.
    /// </summary>
    public interface IQuotationService
    {
        /// <summary>
        /// Teklifi kimliğine göre getirir.
        /// </summary>
        /// <param name="id">Teklif kimliği.</param>
        /// <returns>Teklif bilgileri.</returns>
        Task<QuotationDto?> GetByIdAsync(int id);

        /// <summary>
        /// Teklif detayını ilişkili verilerle birlikte getirir.
        /// </summary>
        /// <param name="id">Teklif kimliği.</param>
        /// <returns>Teklif detay bilgileri.</returns>
        Task<QuotationDto?> GetDetailAsync(int id);

        /// <summary>
        /// Tüm teklifleri listeler.
        /// </summary>
        /// <returns>Teklif listesi.</returns>
        Task<IEnumerable<QuotationListDto>> GetAllAsync();

        /// <summary>
        /// RFQ'ya ait teklifleri listeler.
        /// </summary>
        /// <param name="rfqId">RFQ kimliği.</param>
        /// <returns>Teklif listesi.</returns>
        Task<IEnumerable<QuotationListDto>> GetByRfqAsync(int rfqId);

        /// <summary>
        /// Şirkete ait teklifleri listeler.
        /// </summary>
        /// <param name="companyId">Şirket kimliği.</param>
        /// <returns>Teklif listesi.</returns>
        Task<IEnumerable<QuotationListDto>> GetByCompanyAsync(int companyId);

        /// <summary>
        /// Duruma göre teklifleri listeler.
        /// </summary>
        /// <param name="status">Teklif durumu.</param>
        /// <returns>Teklif listesi.</returns>
        Task<IEnumerable<QuotationListDto>> GetByStatusAsync(QuotationStatus status);

        /// <summary>
        /// Yeni teklif oluşturur.
        /// </summary>
        /// <param name="companyId">Teklifi veren şirket kimliği.</param>
        /// <param name="dto">Teklif bilgileri.</param>
        /// <returns>Oluşturulan teklif.</returns>
        Task<QuotationDto> CreateAsync(int companyId, QuotationCreateDto dto);

        /// <summary>
        /// Teklifi günceller.
        /// </summary>
        /// <param name="id">Teklif kimliği.</param>
        /// <param name="dto">Güncellenecek bilgiler.</param>
        Task UpdateAsync(int id, QuotationCreateDto dto);

        /// <summary>
        /// Teklifi siler (soft delete).
        /// </summary>
        /// <param name="id">Teklif kimliği.</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Benzersiz teklif numarası oluşturur.
        /// Format: QUO-YYYY-XXXXX
        /// </summary>
        /// <returns>Yeni teklif numarası.</returns>
        Task<string> GenerateQuotationNumberAsync();

        /// <summary>
        /// Teklifi gönderir.
        /// Taslak durumundan gönderilmiş durumuna geçirir.
        /// </summary>
        /// <param name="quotationId">Teklif kimliği.</param>
        Task SubmitAsync(int quotationId);

        /// <summary>
        /// Teklifi geri çeker.
        /// </summary>
        /// <param name="quotationId">Teklif kimliği.</param>
        Task WithdrawAsync(int quotationId);

        /// <summary>
        /// RFQ için teklif karşılaştırma matrisini getirir.
        /// </summary>
        /// <param name="rfqId">RFQ kimliği.</param>
        /// <returns>Karşılaştırma matrisi.</returns>
        Task<QuotationComparisonDto> GetComparisonMatrixAsync(int rfqId);

        /// <summary>
        /// Teklif kalemini onaylar.
        /// </summary>
        /// <param name="quotationItemId">Teklif kalem kimliği.</param>
        /// <param name="approvedQuantity">Onaylanan miktar (null ise tamamı onaylanır).</param>
        Task ApproveItemAsync(int quotationItemId, decimal? approvedQuantity = null);

        /// <summary>
        /// Teklif kalemini reddeder.
        /// </summary>
        /// <param name="quotationItemId">Teklif kalem kimliği.</param>
        /// <param name="reason">Red nedeni.</param>
        Task RejectItemAsync(int quotationItemId, string? reason = null);

        /// <summary>
        /// Tüm teklif kalemlerini toplu onaylar.
        /// </summary>
        /// <param name="quotationId">Teklif kimliği.</param>
        Task ApproveAllItemsAsync(int quotationId);

        /// <summary>
        /// Teklifi tamamlar (sipariş oluşturma için hazır).
        /// </summary>
        /// <param name="quotationId">Teklif kimliği.</param>
        Task FinalizeAsync(int quotationId);

        /// <summary>
        /// Teklif kalemi ekler.
        /// </summary>
        /// <param name="quotationId">Teklif kimliği.</param>
        /// <param name="item">Kalem bilgileri.</param>
        Task AddItemAsync(int quotationId, QuotationItemCreateDto item);

        /// <summary>
        /// Teklif kalemini günceller.
        /// </summary>
        /// <param name="itemId">Kalem kimliği.</param>
        /// <param name="item">Güncellenecek bilgiler.</param>
        Task UpdateItemAsync(int itemId, QuotationItemCreateDto item);

        /// <summary>
        /// Teklif kalemini siler.
        /// </summary>
        /// <param name="itemId">Kalem kimliği.</param>
        Task DeleteItemAsync(int itemId);

        /// <summary>
        /// Teklif toplam tutarını yeniden hesaplar.
        /// </summary>
        /// <param name="quotationId">Teklif kimliği.</param>
        Task RecalculateTotalAsync(int quotationId);
    }
}
