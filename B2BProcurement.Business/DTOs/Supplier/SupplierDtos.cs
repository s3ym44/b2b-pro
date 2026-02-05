using System.ComponentModel.DataAnnotations;

namespace B2BProcurement.Business.DTOs.Supplier
{
    /// <summary>
    /// Tedarikçi detay DTO'su.
    /// </summary>
    public class SupplierDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? TaxNumber { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int SectorId { get; set; }
        public string? SectorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Tedarikçi oluşturma DTO'su.
    /// </summary>
    public class SupplierCreateDto
    {
        [Required(ErrorMessage = "Tedarikçi adı zorunludur.")]
        [MaxLength(200, ErrorMessage = "Tedarikçi adı en fazla 200 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "Vergi numarası en fazla 20 karakter olabilir.")]
        public string? TaxNumber { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [MaxLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [MaxLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        public string? Phone { get; set; }

        [MaxLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir.")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Sektör seçimi zorunludur.")]
        public int SectorId { get; set; }

        /// <summary>
        /// Platform üyesi şirket ID (varsa).
        /// </summary>
        public int? SupplierCompanyId { get; set; }
    }

    /// <summary>
    /// Tedarikçi güncelleme DTO'su.
    /// </summary>
    public class SupplierUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tedarikçi adı zorunludur.")]
        [MaxLength(200, ErrorMessage = "Tedarikçi adı en fazla 200 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "Vergi numarası en fazla 20 karakter olabilir.")]
        public string? TaxNumber { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [MaxLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [MaxLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        public string? Phone { get; set; }

        [MaxLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir.")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Sektör seçimi zorunludur.")]
        public int SectorId { get; set; }

        /// <summary>
        /// Platform üyesi şirket ID (varsa).
        /// </summary>
        public int? SupplierCompanyId { get; set; }
    }

    /// <summary>
    /// Tedarikçi liste DTO'su.
    /// </summary>
    public class SupplierListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? SectorName { get; set; }
        public bool IsActive { get; set; }
    }
}
