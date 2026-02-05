using System.ComponentModel.DataAnnotations;

namespace B2BProcurement.Business.DTOs.Material
{
    /// <summary>
    /// Malzeme detay DTO'su.
    /// </summary>
    public class MaterialDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int SectorId { get; set; }
        public string? SectorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Malzeme oluşturma DTO'su.
    /// </summary>
    public class MaterialCreateDto
    {
        [Required(ErrorMessage = "Malzeme kodu zorunludur.")]
        [MaxLength(50, ErrorMessage = "Malzeme kodu en fazla 50 karakter olabilir.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Malzeme adı zorunludur.")]
        [MaxLength(200, ErrorMessage = "Malzeme adı en fazla 200 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Birim zorunludur.")]
        [MaxLength(20, ErrorMessage = "Birim en fazla 20 karakter olabilir.")]
        public string Unit { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sektör seçimi zorunludur.")]
        public int SectorId { get; set; }

        public bool IsPublic { get; set; }
    }

    /// <summary>
    /// Malzeme güncelleme DTO'su.
    /// </summary>
    public class MaterialUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Malzeme kodu zorunludur.")]
        [MaxLength(50, ErrorMessage = "Malzeme kodu en fazla 50 karakter olabilir.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Malzeme adı zorunludur.")]
        [MaxLength(200, ErrorMessage = "Malzeme adı en fazla 200 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Birim zorunludur.")]
        [MaxLength(20, ErrorMessage = "Birim en fazla 20 karakter olabilir.")]
        public string Unit { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sektör seçimi zorunludur.")]
        public int SectorId { get; set; }

        public bool IsPublic { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Malzeme liste DTO'su (özet bilgi).
    /// </summary>
    public class MaterialListDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string? SectorName { get; set; }
        public bool IsActive { get; set; }
    }
}
