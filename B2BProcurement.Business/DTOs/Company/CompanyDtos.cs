using System.ComponentModel.DataAnnotations;

namespace B2BProcurement.Business.DTOs.Company
{
    /// <summary>
    /// Şirket detay DTO'su.
    /// </summary>
    public class CompanyDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string? TaxOffice { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int SectorId { get; set; }
        public string? SectorName { get; set; }
        public int PackageId { get; set; }
        public string? PackageName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Şirket oluşturma DTO'su.
    /// </summary>
    public class CompanyCreateDto
    {
        [Required(ErrorMessage = "Şirket adı zorunludur.")]
        [MaxLength(200, ErrorMessage = "Şirket adı en fazla 200 karakter olabilir.")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vergi numarası zorunludur.")]
        [MaxLength(20, ErrorMessage = "Vergi numarası en fazla 20 karakter olabilir.")]
        public string TaxNumber { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Vergi dairesi en fazla 100 karakter olabilir.")]
        public string? TaxOffice { get; set; }

        [MaxLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir.")]
        public string? Address { get; set; }

        [MaxLength(100, ErrorMessage = "Şehir en fazla 100 karakter olabilir.")]
        public string? City { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [MaxLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [MaxLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Sektör seçimi zorunludur.")]
        public int SectorId { get; set; }

        [Required(ErrorMessage = "Paket seçimi zorunludur.")]
        public int PackageId { get; set; }
    }

    /// <summary>
    /// Şirket güncelleme DTO'su.
    /// </summary>
    public class CompanyUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Şirket adı zorunludur.")]
        [MaxLength(200, ErrorMessage = "Şirket adı en fazla 200 karakter olabilir.")]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Vergi dairesi en fazla 100 karakter olabilir.")]
        public string? TaxOffice { get; set; }

        [MaxLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir.")]
        public string? Address { get; set; }

        [MaxLength(100, ErrorMessage = "Şehir en fazla 100 karakter olabilir.")]
        public string? City { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [MaxLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [MaxLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Sektör seçimi zorunludur.")]
        public int SectorId { get; set; }

        [Required(ErrorMessage = "Paket seçimi zorunludur.")]
        public int PackageId { get; set; }
    }

    /// <summary>
    /// Şirket liste DTO'su (özet bilgi).
    /// </summary>
    public class CompanyListDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? SectorName { get; set; }
        public string? PackageName { get; set; }
        public bool IsActive { get; set; }
    }
}
