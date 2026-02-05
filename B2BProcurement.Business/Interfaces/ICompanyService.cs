using B2BProcurement.Business.DTOs.Company;

namespace B2BProcurement.Business.Interfaces
{
    /// <summary>
    /// Şirket servisi arayüzü.
    /// </summary>
    public interface ICompanyService
    {
        /// <summary>
        /// Şirketi kimliğine göre getirir.
        /// </summary>
        /// <param name="id">Şirket kimliği.</param>
        /// <returns>Şirket bilgileri.</returns>
        Task<CompanyDto?> GetByIdAsync(int id);

        /// <summary>
        /// Tüm şirketleri listeler.
        /// </summary>
        /// <returns>Şirket listesi.</returns>
        Task<IEnumerable<CompanyListDto>> GetAllAsync();

        /// <summary>
        /// Sektöre göre şirketleri listeler.
        /// </summary>
        /// <param name="sectorId">Sektör kimliği.</param>
        /// <returns>Şirket listesi.</returns>
        Task<IEnumerable<CompanyListDto>> GetBySectorAsync(int sectorId);

        /// <summary>
        /// Yeni şirket oluşturur.
        /// </summary>
        /// <param name="dto">Şirket bilgileri.</param>
        /// <returns>Oluşturulan şirket.</returns>
        Task<CompanyDto> CreateAsync(CompanyCreateDto dto);

        /// <summary>
        /// Şirket bilgilerini günceller.
        /// </summary>
        /// <param name="id">Şirket kimliği.</param>
        /// <param name="dto">Güncellenecek bilgiler.</param>
        Task UpdateAsync(int id, CompanyUpdateDto dto);

        /// <summary>
        /// Şirketi siler (soft delete).
        /// </summary>
        /// <param name="id">Şirket kimliği.</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Şirketin paket limitini kontrol eder.
        /// </summary>
        /// <param name="companyId">Şirket kimliği.</param>
        /// <param name="limitType">Limit tipi (Users, Materials, Rfq).</param>
        /// <returns>Limit aşılmamışsa true.</returns>
        Task<bool> CheckPackageLimitAsync(int companyId, string limitType);

        /// <summary>
        /// Şirketin paket kullanım bilgilerini getirir.
        /// </summary>
        /// <param name="companyId">Şirket kimliği.</param>
        /// <returns>Paket kullanım bilgileri.</returns>
        Task<PackageUsageDto> GetPackageUsageAsync(int companyId);

        /// <summary>
        /// Vergi numarasına göre şirket var mı kontrol eder.
        /// </summary>
        /// <param name="taxNumber">Vergi numarası.</param>
        /// <returns>Varsa true.</returns>
        Task<bool> ExistsByTaxNumberAsync(string taxNumber);
    }

    /// <summary>
    /// Paket kullanım bilgileri DTO'su.
    /// </summary>
    public class PackageUsageDto
    {
        public string PackageName { get; set; } = string.Empty;
        public int MaxUsers { get; set; }
        public int CurrentUsers { get; set; }
        public int MaxMaterials { get; set; }
        public int CurrentMaterials { get; set; }
        public int MaxRfqPerMonth { get; set; }
        public int CurrentRfqThisMonth { get; set; }
        public bool CanUseSapIntegration { get; set; }
    }
}
