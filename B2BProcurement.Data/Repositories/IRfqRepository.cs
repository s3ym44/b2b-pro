using B2BProcurement.Core.Entities;
using B2BProcurement.Core.Enums;

namespace B2BProcurement.Data.Repositories
{
    /// <summary>
    /// RFQ için özelleştirilmiş repository arayüzü.
    /// </summary>
    public interface IRfqRepository : IRepository<RFQ>
    {
        /// <summary>
        /// RFQ'yu ilişkili kalemler, dokümanlar ve kişilerle birlikte getirir.
        /// </summary>
        /// <param name="id">RFQ kimliği.</param>
        /// <returns>İlişkili verilerle birlikte RFQ.</returns>
        Task<RFQ?> GetWithItemsAsync(int id);

        /// <summary>
        /// Şirkete ait RFQ'ları getirir.
        /// </summary>
        /// <param name="companyId">Şirket kimliği.</param>
        /// <returns>Şirkete ait RFQ listesi.</returns>
        Task<IEnumerable<RFQ>> GetByCompanyAsync(int companyId);

        /// <summary>
        /// Sektöre ait açık RFQ'ları getirir.
        /// </summary>
        /// <param name="sectorId">Sektör kimliği.</param>
        /// <returns>Sektöre ait RFQ listesi.</returns>
        Task<IEnumerable<RFQ>> GetBySectorAsync(int sectorId);

        /// <summary>
        /// Belirli duruma sahip RFQ'ları getirir.
        /// </summary>
        /// <param name="status">RFQ durumu.</param>
        /// <returns>İlgili duruma sahip RFQ listesi.</returns>
        Task<IEnumerable<RFQ>> GetByStatusAsync(RfqStatus status);

        /// <summary>
        /// Süresi dolmuş RFQ'ları getirir.
        /// </summary>
        /// <returns>Süresi dolmuş RFQ listesi.</returns>
        Task<IEnumerable<RFQ>> GetExpiredAsync();
    }
}
