using B2BProcurement.Business.DTOs.Material;

namespace B2BProcurement.Business.Interfaces
{
    /// <summary>
    /// Malzeme servisi arayüzü.
    /// </summary>
    public interface IMaterialService
    {
        /// <summary>
        /// Malzemeyi kimliğine göre getirir.
        /// </summary>
        /// <param name="id">Malzeme kimliği.</param>
        /// <returns>Malzeme bilgileri.</returns>
        Task<MaterialDto?> GetByIdAsync(int id);

        /// <summary>
        /// Tüm malzemeleri listeler.
        /// </summary>
        /// <returns>Malzeme listesi.</returns>
        Task<IEnumerable<MaterialListDto>> GetAllAsync();

        /// <summary>
        /// Şirkete ait malzemeleri listeler.
        /// </summary>
        /// <param name="companyId">Şirket kimliği.</param>
        /// <returns>Malzeme listesi.</returns>
        Task<IEnumerable<MaterialListDto>> GetByCompanyAsync(int companyId);

        /// <summary>
        /// Sektöre göre malzemeleri listeler.
        /// </summary>
        /// <param name="sectorId">Sektör kimliği.</param>
        /// <returns>Malzeme listesi.</returns>
        Task<IEnumerable<MaterialListDto>> GetBySectorAsync(int sectorId);

        /// <summary>
        /// Yeni malzeme oluşturur.
        /// </summary>
        /// <param name="companyId">Şirket kimliği.</param>
        /// <param name="dto">Malzeme bilgileri.</param>
        /// <returns>Oluşturulan malzeme.</returns>
        Task<MaterialDto> CreateAsync(int companyId, MaterialCreateDto dto);

        /// <summary>
        /// Malzeme bilgilerini günceller.
        /// </summary>
        /// <param name="id">Malzeme kimliği.</param>
        /// <param name="dto">Güncellenecek bilgiler.</param>
        Task UpdateAsync(int id, MaterialUpdateDto dto);

        /// <summary>
        /// Malzemeyi siler (soft delete).
        /// </summary>
        /// <param name="id">Malzeme kimliği.</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Genel malzeme araması yapar.
        /// </summary>
        /// <param name="query">Arama sorgusu.</param>
        /// <param name="sectorId">Opsiyonel sektör filtresi.</param>
        /// <returns>Eşleşen malzemeler.</returns>
        Task<IEnumerable<MaterialListDto>> SearchPublicAsync(string query, int? sectorId = null);

        /// <summary>
        /// Malzeme kodunun benzersiz olup olmadığını kontrol eder.
        /// </summary>
        /// <param name="companyId">Şirket kimliği.</param>
        /// <param name="code">Malzeme kodu.</param>
        /// <param name="excludeId">Hariç tutulacak malzeme kimliği (güncelleme için).</param>
        /// <returns>Benzersiz ise true.</returns>
        Task<bool> IsCodeUniqueAsync(int companyId, string code, int? excludeId = null);
    }
}
