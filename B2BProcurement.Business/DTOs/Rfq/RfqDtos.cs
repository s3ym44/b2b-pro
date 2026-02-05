using System.ComponentModel.DataAnnotations;
using B2BProcurement.Core.Enums;

namespace B2BProcurement.Business.DTOs.Rfq
{
    /// <summary>
    /// RFQ detay DTO'su.
    /// </summary>
    public class RfqDto
    {
        public int Id { get; set; }
        public string RfqNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int SectorId { get; set; }
        public string? SectorName { get; set; }
        public RfqStatus Status { get; set; }
        public RfqVisibility Visibility { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = "TRY";
        public int ItemCount { get; set; }
        public int QuotationCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// RFQ liste DTO'su (özet bilgi).
    /// </summary>
    public class RfqListDto
    {
        public int Id { get; set; }
        public string RfqNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? SectorName { get; set; }
        public RfqStatus Status { get; set; }
        public DateTime EndDate { get; set; }
        public int ItemCount { get; set; }
        public int QuotationCount { get; set; }
    }

    /// <summary>
    /// RFQ detay DTO'su (kalemler ve dokümanlar dahil).
    /// </summary>
    public class RfqDetailDto
    {
        public int Id { get; set; }
        public string RfqNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int SectorId { get; set; }
        public string? SectorName { get; set; }
        public RfqStatus Status { get; set; }
        public RfqVisibility Visibility { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = "TRY";
        public DateTime CreatedAt { get; set; }

        public List<RfqItemDto> Items { get; set; } = new();
        public List<RfqContactDto> Contacts { get; set; } = new();
    }

    /// <summary>
    /// RFQ oluşturma DTO'su.
    /// </summary>
    public class RfqCreateDto
    {
        [Required(ErrorMessage = "RFQ başlığı zorunludur.")]
        [MaxLength(500, ErrorMessage = "Başlık en fazla 500 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sektör seçimi zorunludur.")]
        public int SectorId { get; set; }

        [Required(ErrorMessage = "Görünürlük seçimi zorunludur.")]
        public RfqVisibility Visibility { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        public DateTime EndDate { get; set; }

        [MaxLength(10, ErrorMessage = "Para birimi en fazla 10 karakter olabilir.")]
        public string Currency { get; set; } = "TRY";

        [Required(ErrorMessage = "En az bir kalem eklemelisiniz.")]
        [MinLength(1, ErrorMessage = "En az bir kalem eklemelisiniz.")]
        public List<RfqItemCreateDto> Items { get; set; } = new();

        public List<RfqContactCreateDto>? Contacts { get; set; }
    }

    /// <summary>
    /// RFQ güncelleme DTO'su.
    /// </summary>
    public class RfqUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "RFQ başlığı zorunludur.")]
        [MaxLength(500, ErrorMessage = "Başlık en fazla 500 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sektör seçimi zorunludur.")]
        public int SectorId { get; set; }

        [Required(ErrorMessage = "Görünürlük seçimi zorunludur.")]
        public RfqVisibility Visibility { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        public DateTime EndDate { get; set; }

        [MaxLength(10, ErrorMessage = "Para birimi en fazla 10 karakter olabilir.")]
        public string Currency { get; set; } = "TRY";
    }

    /// <summary>
    /// RFQ kalemi DTO'su.
    /// </summary>
    public class RfqItemDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string? TechnicalSpecs { get; set; }
        public int? MaterialId { get; set; }
        public string? MaterialName { get; set; }
    }

    /// <summary>
    /// RFQ kalemi oluşturma DTO'su.
    /// </summary>
    public class RfqItemCreateDto
    {
        [Required(ErrorMessage = "Kalem açıklaması zorunludur.")]
        [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Miktar zorunludur.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "Birim zorunludur.")]
        [MaxLength(20, ErrorMessage = "Birim en fazla 20 karakter olabilir.")]
        public string Unit { get; set; } = string.Empty;

        [MaxLength(4000, ErrorMessage = "Teknik özellikler en fazla 4000 karakter olabilir.")]
        public string? TechnicalSpecs { get; set; }

        public int? MaterialId { get; set; }
    }

    /// <summary>
    /// RFQ iletişim kişisi DTO'su.
    /// </summary>
    public class RfqContactDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    /// <summary>
    /// RFQ iletişim kişisi oluşturma DTO'su.
    /// </summary>
    public class RfqContactCreateDto
    {
        [Required(ErrorMessage = "İletişim kişisi adı zorunludur.")]
        [MaxLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [MaxLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [MaxLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        public string? Phone { get; set; }
    }
}
