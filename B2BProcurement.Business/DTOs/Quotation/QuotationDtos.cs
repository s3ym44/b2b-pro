using System.ComponentModel.DataAnnotations;
using B2BProcurement.Core.Enums;

namespace B2BProcurement.Business.DTOs.Quotation
{
    /// <summary>
    /// Teklif detay DTO'su.
    /// </summary>
    public class QuotationDto
    {
        public int Id { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public int RfqId { get; set; }
        public string? RfqNumber { get; set; }
        public string? RfqTitle { get; set; }
        public int SupplierCompanyId { get; set; }
        public string? SupplierCompanyName { get; set; }
        public QuotationStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Teklif liste DTO'su (özet bilgi).
    /// </summary>
    public class QuotationListDto
    {
        public int Id { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public string? RfqNumber { get; set; }
        public string? SupplierCompanyName { get; set; }
        public QuotationStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int ItemCount { get; set; }
    }

    /// <summary>
    /// Teklif oluşturma DTO'su.
    /// </summary>
    public class QuotationCreateDto
    {
        [Required(ErrorMessage = "RFQ seçimi zorunludur.")]
        public int RfqId { get; set; }

        public DateTime? ValidUntil { get; set; }

        [Required(ErrorMessage = "En az bir kalem eklemelisiniz.")]
        [MinLength(1, ErrorMessage = "En az bir kalem eklemelisiniz.")]
        public List<QuotationItemCreateDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Teklif gönderme DTO'su.
    /// </summary>
    public class QuotationSubmitDto
    {
        [Required]
        public int Id { get; set; }

        public string? Notes { get; set; }
    }

    /// <summary>
    /// Teklif kalemi DTO'su.
    /// </summary>
    public class QuotationItemDto
    {
        public int Id { get; set; }
        public int RfqItemId { get; set; }
        public string? RfqItemDescription { get; set; }
        public decimal RequestedQuantity { get; set; }
        public string? Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OfferedQuantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public decimal? ApprovedQuantity { get; set; }
    }

    /// <summary>
    /// Teklif kalemi oluşturma DTO'su.
    /// </summary>
    public class QuotationItemCreateDto
    {
        [Required(ErrorMessage = "RFQ kalemi seçimi zorunludur.")]
        public int RfqItemId { get; set; }

        [Required(ErrorMessage = "Birim fiyat zorunludur.")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Birim fiyat 0'dan büyük olmalıdır.")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Teklif edilen miktar zorunludur.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır.")]
        public decimal OfferedQuantity { get; set; }

        public DateTime? DeliveryDate { get; set; }
    }

    /// <summary>
    /// Teklif karşılaştırma DTO'su (matris için).
    /// </summary>
    public class QuotationComparisonDto
    {
        public int RfqId { get; set; }
        public string? RfqNumber { get; set; }
        public string? RfqTitle { get; set; }

        public List<ComparisonItemDto> Items { get; set; } = new();
        public List<ComparisonSupplierDto> Suppliers { get; set; } = new();
    }

    /// <summary>
    /// Karşılaştırma matrisi kalem DTO'su.
    /// </summary>
    public class ComparisonItemDto
    {
        public int RfqItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal RequestedQuantity { get; set; }
        public string Unit { get; set; } = string.Empty;

        public List<ComparisonPriceDto> Prices { get; set; } = new();
    }

    /// <summary>
    /// Karşılaştırma matrisi tedarikçi DTO'su.
    /// </summary>
    public class ComparisonSupplierDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string QuotationNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Karşılaştırma matrisi fiyat DTO'su.
    /// </summary>
    public class ComparisonPriceDto
    {
        public int SupplierCompanyId { get; set; }
        public string? SupplierName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OfferedQuantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public bool IsLowestPrice { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
    }
}
