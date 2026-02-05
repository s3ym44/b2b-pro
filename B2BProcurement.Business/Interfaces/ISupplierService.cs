using B2BProcurement.Business.DTOs.Supplier;

namespace B2BProcurement.Business.Interfaces
{
    /// <summary>
    /// Tedarikçi servisi arayüzü.
    /// </summary>
    public interface ISupplierService
    {
        /// <summary>
        /// Tedarikçiyi kimliğine göre getirir.
        /// </summary>
        /// <param name="id">Tedarikçi kimliği.</param>
        /// <returns>Tedarikçi bilgileri.</returns>
        Task<SupplierDto?> GetByIdAsync(int id);

        /// <summary>
        /// Tüm tedarikçileri listeler.
        /// </summary>
        /// <returns>Tedarikçi listesi.</returns>
        Task<IEnumerable<SupplierListDto>> GetAllAsync();

        /// <summary>
        /// Şirkete ait tedarikçileri listeler.
        /// </summary>
        /// <param name="companyId">Şirket kimliği.</param>
        /// <returns>Tedarikçi listesi.</returns>
        Task<IEnumerable<SupplierListDto>> GetByCompanyAsync(int companyId);

        /// <summary>
        /// Sektöre göre tedarikçileri listeler.
        /// </summary>
        /// <param name="sectorId">Sektör kimliği.</param>
        /// <returns>Tedarikçi listesi.</returns>
        Task<IEnumerable<SupplierListDto>> GetBySectorAsync(int sectorId);

        /// <summary>
        /// Yeni tedarikçi oluşturur.
        /// </summary>
        /// <param name="companyId">Şirket kimliği.</param>
        /// <param name="dto">Tedarikçi bilgileri.</param>
        /// <returns>Oluşturulan tedarikçi.</returns>
        Task<SupplierDto> CreateAsync(int companyId, SupplierCreateDto dto);

        /// <summary>
        /// Tedarikçi bilgilerini günceller.
        /// </summary>
        /// <param name="id">Tedarikçi kimliği.</param>
        /// <param name="dto">Güncellenecek bilgiler.</param>
        Task UpdateAsync(int id, SupplierUpdateDto dto);

        /// <summary>
        /// Tedarikçiyi siler (soft delete).
        /// </summary>
        /// <param name="id">Tedarikçi kimliği.</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Tedarikçi araması yapar.
        /// </summary>
        /// <param name="companyId">Şirket kimliği.</param>
        /// <param name="query">Arama sorgusu.</param>
        /// <returns>Eşleşen tedarikçiler.</returns>
        Task<IEnumerable<SupplierListDto>> SearchAsync(int companyId, string query);
    }
}
