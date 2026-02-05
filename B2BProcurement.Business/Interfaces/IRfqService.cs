using B2BProcurement.Business.DTOs.Rfq;
using B2BProcurement.Core.Enums;

namespace B2BProcurement.Business.Interfaces
{
    /// <summary>
    /// RFQ (Teklif Talebi) servisi arayüzü.
    /// </summary>
    public interface IRfqService
    {
        /// <summary>
        /// RFQ'yu kimliğine göre getirir.
        /// </summary>
        /// <param name="id">RFQ kimliği.</param>
        /// <returns>RFQ bilgileri.</returns>
        Task<RfqDto?> GetByIdAsync(int id);

        /// <summary>
        /// RFQ detayını ilişkili verilerle birlikte getirir.
        /// </summary>
        /// <param name="id">RFQ kimliği.</param>
        /// <returns>RFQ detay bilgileri.</returns>
        Task<RfqDetailDto?> GetDetailAsync(int id);

        /// <summary>
        /// Tüm RFQ'ları listeler.
        /// </summary>
        /// <returns>RFQ listesi.</returns>
        Task<IEnumerable<RfqListDto>> GetAllAsync();

        /// <summary>
        /// Şirkete ait RFQ'ları listeler.
        /// </summary>
        /// <param name="companyId">Şirket kimliği.</param>
        /// <returns>RFQ listesi.</returns>
        Task<IEnumerable<RfqListDto>> GetByCompanyAsync(int companyId);

        /// <summary>
        /// Sektöre göre açık RFQ'ları listeler.
        /// </summary>
        /// <param name="sectorId">Sektör kimliği.</param>
        /// <returns>RFQ listesi.</returns>
        Task<IEnumerable<RfqListDto>> GetBySectorAsync(int sectorId);

        /// <summary>
        /// Duruma göre RFQ'ları listeler.
        /// </summary>
        /// <param name="status">RFQ durumu.</param>
        /// <returns>RFQ listesi.</returns>
        Task<IEnumerable<RfqListDto>> GetByStatusAsync(RfqStatus status);

        /// <summary>
        /// Şirkete gelen RFQ'ları listeler (sektöre göre).
        /// Tedarikçi olarak katılabileceği açık RFQ'lar.
        /// </summary>
        /// <param name="companyId">Şirket kimliği.</param>
        /// <returns>Gelen RFQ listesi.</returns>
        Task<IEnumerable<RfqListDto>> GetIncomingRfqsAsync(int companyId);

        /// <summary>
        /// Yeni RFQ oluşturur.
        /// </summary>
        /// <param name="companyId">Şirket kimliği.</param>
        /// <param name="dto">RFQ bilgileri.</param>
        /// <returns>Oluşturulan RFQ.</returns>
        Task<RfqDto> CreateAsync(int companyId, RfqCreateDto dto);

        /// <summary>
        /// RFQ bilgilerini günceller.
        /// </summary>
        /// <param name="id">RFQ kimliği.</param>
        /// <param name="dto">Güncellenecek bilgiler.</param>
        Task UpdateAsync(int id, RfqUpdateDto dto);

        /// <summary>
        /// RFQ'yu siler (soft delete).
        /// </summary>
        /// <param name="id">RFQ kimliği.</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Benzersiz RFQ numarası oluşturur.
        /// Format: RFQ-YYYY-XXXXX
        /// </summary>
        /// <returns>Yeni RFQ numarası.</returns>
        Task<string> GenerateRfqNumberAsync();

        /// <summary>
        /// RFQ'yu yayınlar.
        /// Taslak durumundan yayınlanmış durumuna geçirir.
        /// </summary>
        /// <param name="rfqId">RFQ kimliği.</param>
        Task PublishAsync(int rfqId);

        /// <summary>
        /// RFQ'yu kapatır.
        /// Yeni teklif kabul etmez, değerlendirme aşamasına geçer.
        /// </summary>
        /// <param name="rfqId">RFQ kimliği.</param>
        Task CloseAsync(int rfqId);

        /// <summary>
        /// RFQ'yu iptal eder.
        /// </summary>
        /// <param name="rfqId">RFQ kimliği.</param>
        Task CancelAsync(int rfqId);

        /// <summary>
        /// Süresi dolmuş RFQ'ları otomatik kapatır.
        /// </summary>
        /// <returns>Kapatılan RFQ sayısı.</returns>
        Task<int> CloseExpiredRfqsAsync();

        /// <summary>
        /// RFQ'ya kalem ekler.
        /// </summary>
        /// <param name="rfqId">RFQ kimliği.</param>
        /// <param name="item">Kalem bilgileri.</param>
        Task AddItemAsync(int rfqId, RfqItemCreateDto item);

        /// <summary>
        /// RFQ kalemini günceller.
        /// </summary>
        /// <param name="itemId">Kalem kimliği.</param>
        /// <param name="item">Güncellenecek bilgiler.</param>
        Task UpdateItemAsync(int itemId, RfqItemCreateDto item);

        /// <summary>
        /// RFQ kalemini siler.
        /// </summary>
        /// <param name="itemId">Kalem kimliği.</param>
        Task DeleteItemAsync(int itemId);
    }
}
