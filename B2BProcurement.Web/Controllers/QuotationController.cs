using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Business.DTOs.Quotation;
using B2BProcurement.Core.Enums;
using B2BProcurement.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace B2BProcurement.Controllers
{
    /// <summary>
    /// Teklif (Quotation) yönetimi controller'ı.
    /// Tedarikçilerin RFQ'lara verdikleri teklifleri yönetir.
    /// </summary>
    [Authorize]
    public class QuotationController : Controller
    {
        private readonly IQuotationService _quotationService;
        private readonly IRfqService _rfqService;
        private readonly ApplicationDbContext _context;

        public QuotationController(
            IQuotationService quotationService,
            IRfqService rfqService,
            ApplicationDbContext context)
        {
            _quotationService = quotationService;
            _rfqService = rfqService;
            _context = context;
        }

        #region Helper Methods

        private int GetCurrentCompanyId()
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            return int.TryParse(companyIdClaim, out var companyId) ? companyId : 0;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        #endregion

        #region Index - Verdiğim Teklifler

        /// <summary>
        /// Kullanıcının şirketinin verdiği tekliflerin listesi.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var quotations = await _quotationService.GetByCompanyAsync(companyId);
            
            var viewModel = quotations.Select(q => new QuotationListItemViewModel
            {
                Id = q.Id,
                QuotationNumber = q.QuotationNumber,
                RfqNumber = q.RfqNumber ?? string.Empty,
                Status = q.Status,
                TotalAmount = q.TotalAmount,
                ValidUntil = q.ValidUntil,
                ItemCount = q.ItemCount
            }).ToList();

            return View(viewModel);
        }

        #endregion

        #region Details

        /// <summary>
        /// Teklif detayı.
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var quotation = await _quotationService.GetDetailAsync(id);
            if (quotation == null)
            {
                return NotFound();
            }

            // Yetki kontrolü
            var companyId = GetCurrentCompanyId();
            var isOwner = quotation.SupplierCompanyId == companyId;
            
            // RFQ sahibi de görebilir
            var rfq = await _context.RFQs
                .Where(r => r.Id == quotation.RfqId)
                .Select(r => new { r.CompanyId })
                .FirstOrDefaultAsync();
            
            var isRfqOwner = rfq?.CompanyId == companyId;

            if (!isOwner && !isRfqOwner)
            {
                return Forbid();
            }

            // Quotation Items'ları getir
            var items = await _context.QuotationItems
                .Include(qi => qi.RFQItem)
                .Where(qi => qi.QuotationId == id && qi.IsActive)
                .Select(qi => new QuotationItemViewModel
                {
                    Id = qi.Id,
                    RfqItemDescription = qi.RFQItem != null ? qi.RFQItem.Description : "",
                    RequestedQuantity = qi.RFQItem != null ? qi.RFQItem.Quantity : 0,
                    Unit = qi.RFQItem != null ? qi.RFQItem.Unit : "",
                    UnitPrice = qi.UnitPrice,
                    OfferedQuantity = qi.OfferedQuantity,
                    TotalPrice = qi.TotalPrice,
                    DeliveryDate = qi.DeliveryDate,
                    ApprovalStatus = qi.ApprovalStatus,
                    ApprovedQuantity = qi.ApprovedQuantity
                })
                .ToListAsync();

            var viewModel = new QuotationDetailsViewModel
            {
                Id = quotation.Id,
                QuotationNumber = quotation.QuotationNumber,
                RfqId = quotation.RfqId,
                RfqNumber = quotation.RfqNumber ?? string.Empty,
                RfqTitle = quotation.RfqTitle ?? string.Empty,
                SupplierCompanyName = quotation.SupplierCompanyName ?? string.Empty,
                Status = quotation.Status,
                TotalAmount = quotation.TotalAmount,
                ValidUntil = quotation.ValidUntil,
                CreatedAt = quotation.CreatedAt,
                IsOwner = isOwner,
                IsRfqOwner = isRfqOwner,
                Items = items
            };

            return View(viewModel);
        }

        #endregion

        #region Create - RFQ'ya Cevap Ver

        /// <summary>
        /// RFQ'ya yeni teklif oluşturma formu.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create(int rfqId)
        {
            var rfq = await _context.RFQs
                .Include(r => r.Items.Where(i => i.IsActive))
                .ThenInclude(i => i.Material)
                .Include(r => r.Sector)
                .Where(r => r.Id == rfqId && r.Status == RfqStatus.Published && r.IsActive)
                .FirstOrDefaultAsync();

            if (rfq == null)
            {
                TempData["Error"] = "RFQ bulunamadı veya teklif verilemez durumda.";
                return RedirectToAction("Incoming", "Rfq");
            }

            // Daha önce teklif verilmiş mi kontrol
            var companyId = GetCurrentCompanyId();
            var existingQuotation = await _context.Quotations
                .Where(q => q.RfqId == rfqId && q.SupplierCompanyId == companyId && q.IsActive)
                .FirstOrDefaultAsync();

            if (existingQuotation != null)
            {
                TempData["Warning"] = "Bu RFQ'ya zaten teklif vermişsiniz.";
                return RedirectToAction("Edit", new { id = existingQuotation.Id });
            }

            var viewModel = new QuotationFormViewModel
            {
                RfqId = rfq.Id,
                RfqNumber = rfq.RfqNumber,
                RfqTitle = rfq.Title,
                RfqEndDate = rfq.EndDate,
                Currency = rfq.Currency,
                RfqItems = rfq.Items.Select(i => new RfqItemForQuotationViewModel
                {
                    RfqItemId = i.Id,
                    Description = i.Description,
                    MaterialName = i.Material?.Name,
                    RequestedQuantity = i.Quantity,
                    Unit = i.Unit,
                    TechnicalSpecs = i.TechnicalSpecs
                }).ToList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Yeni teklif oluşturma işlemi.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuotationFormViewModel model)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            // Parse items from JSON
            if (!string.IsNullOrEmpty(model.ItemsJson))
            {
                try
                {
                    model.Items = System.Text.Json.JsonSerializer.Deserialize<List<QuotationItemInputModel>>(model.ItemsJson)
                        ?? new List<QuotationItemInputModel>();
                }
                catch
                {
                    ModelState.AddModelError("", "Kalem bilgileri geçersiz.");
                }
            }

            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError("", "En az bir kalem için teklif girmelisiniz.");
            }

            if (!ModelState.IsValid)
            {
                // Reload RFQ items
                await ReloadRfqItems(model);
                return View(model);
            }

            var createDto = new QuotationCreateDto
            {
                RfqId = model.RfqId,
                ValidUntil = model.ValidUntil,
                Items = model.Items!.Select(i => new QuotationItemCreateDto
                {
                    RfqItemId = i.RfqItemId,
                    UnitPrice = i.UnitPrice,
                    OfferedQuantity = i.OfferedQuantity,
                    DeliveryDate = i.DeliveryDate
                }).ToList()
            };

            var quotation = await _quotationService.CreateAsync(companyId, createDto);

            // Eğer hemen gönder seçilmişse
            if (model.SubmitNow)
            {
                await _quotationService.SubmitAsync(quotation.Id);
                TempData["Success"] = "Teklifiniz başarıyla gönderildi.";
            }
            else
            {
                TempData["Success"] = "Teklifiniz taslak olarak kaydedildi.";
            }

            return RedirectToAction("Details", new { id = quotation.Id });
        }

        #endregion

        #region Edit

        /// <summary>
        /// Teklif düzenleme formu.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var quotation = await _quotationService.GetDetailAsync(id);
            if (quotation == null)
            {
                return NotFound();
            }

            // Sadece sahip düzenleyebilir ve taslak durumunda olmalı
            var companyId = GetCurrentCompanyId();
            if (quotation.SupplierCompanyId != companyId)
            {
                return Forbid();
            }

            if (quotation.Status != QuotationStatus.Draft)
            {
                TempData["Error"] = "Sadece taslak durumdaki teklifler düzenlenebilir.";
                return RedirectToAction("Details", new { id });
            }

            // RFQ bilgilerini getir
            var rfq = await _context.RFQs
                .Include(r => r.Items.Where(i => i.IsActive))
                .ThenInclude(i => i.Material)
                .Where(r => r.Id == quotation.RfqId)
                .FirstOrDefaultAsync();

            if (rfq == null)
            {
                return NotFound();
            }

            // Mevcut kalem tekliflerini getir
            var existingItems = await _context.QuotationItems
                .Where(qi => qi.QuotationId == id && qi.IsActive)
                .ToListAsync();

            var viewModel = new QuotationFormViewModel
            {
                Id = quotation.Id,
                RfqId = rfq.Id,
                RfqNumber = rfq.RfqNumber,
                RfqTitle = rfq.Title,
                RfqEndDate = rfq.EndDate,
                Currency = rfq.Currency,
                ValidUntil = quotation.ValidUntil,
                RfqItems = rfq.Items.Select(i => new RfqItemForQuotationViewModel
                {
                    RfqItemId = i.Id,
                    Description = i.Description,
                    MaterialName = i.Material?.Name,
                    RequestedQuantity = i.Quantity,
                    Unit = i.Unit,
                    TechnicalSpecs = i.TechnicalSpecs
                }).ToList(),
                Items = existingItems.Select(ei => new QuotationItemInputModel
                {
                    RfqItemId = ei.RfqItemId,
                    UnitPrice = ei.UnitPrice,
                    OfferedQuantity = ei.OfferedQuantity,
                    DeliveryDate = ei.DeliveryDate
                }).ToList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Teklif güncelleme işlemi.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuotationFormViewModel model)
        {
            var quotation = await _quotationService.GetByIdAsync(id);
            if (quotation == null)
            {
                return NotFound();
            }

            var companyId = GetCurrentCompanyId();
            if (quotation.SupplierCompanyId != companyId)
            {
                return Forbid();
            }

            if (quotation.Status != QuotationStatus.Draft)
            {
                TempData["Error"] = "Sadece taslak durumdaki teklifler düzenlenebilir.";
                return RedirectToAction("Details", new { id });
            }

            // Parse items from JSON
            if (!string.IsNullOrEmpty(model.ItemsJson))
            {
                try
                {
                    model.Items = System.Text.Json.JsonSerializer.Deserialize<List<QuotationItemInputModel>>(model.ItemsJson)
                        ?? new List<QuotationItemInputModel>();
                }
                catch
                {
                    ModelState.AddModelError("", "Kalem bilgileri geçersiz.");
                }
            }

            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError("", "En az bir kalem için teklif girmelisiniz.");
            }

            if (!ModelState.IsValid)
            {
                await ReloadRfqItems(model);
                return View(model);
            }

            var updateDto = new QuotationCreateDto
            {
                RfqId = model.RfqId,
                ValidUntil = model.ValidUntil,
                Items = model.Items!.Select(i => new QuotationItemCreateDto
                {
                    RfqItemId = i.RfqItemId,
                    UnitPrice = i.UnitPrice,
                    OfferedQuantity = i.OfferedQuantity,
                    DeliveryDate = i.DeliveryDate
                }).ToList()
            };

            await _quotationService.UpdateAsync(id, updateDto);

            if (model.SubmitNow)
            {
                await _quotationService.SubmitAsync(id);
                TempData["Success"] = "Teklifiniz başarıyla gönderildi.";
            }
            else
            {
                TempData["Success"] = "Değişiklikler kaydedildi.";
            }

            return RedirectToAction("Details", new { id });
        }

        #endregion

        #region Submit

        /// <summary>
        /// Taslak teklifi gönder.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var quotation = await _quotationService.GetByIdAsync(id);
            if (quotation == null)
            {
                return NotFound();
            }

            var companyId = GetCurrentCompanyId();
            if (quotation.SupplierCompanyId != companyId)
            {
                return Forbid();
            }

            if (quotation.Status != QuotationStatus.Draft)
            {
                TempData["Error"] = "Sadece taslak teklifler gönderilebilir.";
                return RedirectToAction("Details", new { id });
            }

            await _quotationService.SubmitAsync(id);
            TempData["Success"] = "Teklifiniz başarıyla gönderildi.";

            return RedirectToAction("Details", new { id });
        }

        #endregion

        #region Approval Actions (RFQ Owner)

        /// <summary>
        /// Teklif kalemini onayla.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveItem(int quotationId, int itemId)
        {
            await _quotationService.ApproveItemAsync(itemId);
            TempData["Success"] = "Kalem onaylandı.";
            
            // Teklif bilgisini al ve RFQ comparison'a dön
            var quotation = await _quotationService.GetByIdAsync(quotationId);
            if (quotation != null)
            {
                return RedirectToAction("Comparison", "Rfq", new { id = quotation.RfqId });
            }
            
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Teklif kalemini reddet.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectItem(int quotationId, int itemId)
        {
            await _quotationService.RejectItemAsync(itemId);
            TempData["Success"] = "Kalem reddedildi.";
            
            var quotation = await _quotationService.GetByIdAsync(quotationId);
            if (quotation != null)
            {
                return RedirectToAction("Comparison", "Rfq", new { id = quotation.RfqId });
            }
            
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Tüm teklifi onayla.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveQuotation(int quotationId)
        {
            await _quotationService.ApproveAllItemsAsync(quotationId);
            TempData["Success"] = "Tüm kalemler onaylandı.";
            
            var quotation = await _quotationService.GetByIdAsync(quotationId);
            if (quotation != null)
            {
                return RedirectToAction("Comparison", "Rfq", new { id = quotation.RfqId });
            }
            
            return RedirectToAction("Index");
        }

        #endregion

        #region Helper Methods

        private async Task ReloadRfqItems(QuotationFormViewModel model)
        {
            var rfq = await _context.RFQs
                .Include(r => r.Items.Where(i => i.IsActive))
                .ThenInclude(i => i.Material)
                .Where(r => r.Id == model.RfqId)
                .FirstOrDefaultAsync();

            if (rfq != null)
            {
                model.RfqNumber = rfq.RfqNumber;
                model.RfqTitle = rfq.Title;
                model.RfqEndDate = rfq.EndDate;
                model.Currency = rfq.Currency;
                model.RfqItems = rfq.Items.Select(i => new RfqItemForQuotationViewModel
                {
                    RfqItemId = i.Id,
                    Description = i.Description,
                    MaterialName = i.Material?.Name,
                    RequestedQuantity = i.Quantity,
                    Unit = i.Unit,
                    TechnicalSpecs = i.TechnicalSpecs
                }).ToList();
            }
        }

        #endregion
    }

    #region ViewModels

    public class QuotationListItemViewModel
    {
        public int Id { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public string RfqNumber { get; set; } = string.Empty;
        public QuotationStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int ItemCount { get; set; }

        public string StatusText => Status switch
        {
            QuotationStatus.Draft => "Taslak",
            QuotationStatus.Submitted => "Gönderildi",
            QuotationStatus.Approved => "Onaylandı",
            QuotationStatus.Rejected => "Reddedildi",
            QuotationStatus.Withdrawn => "Geri Çekildi",
            QuotationStatus.Completed => "Tamamlandı",
            QuotationStatus.PartiallyApproved => "Kısmi Onay",
            _ => "-"
        };

        public string StatusBadgeClass => Status switch
        {
            QuotationStatus.Draft => "bg-secondary",
            QuotationStatus.Submitted => "bg-primary",
            QuotationStatus.Approved => "bg-success",
            QuotationStatus.Rejected => "bg-danger",
            QuotationStatus.Withdrawn => "bg-warning",
            QuotationStatus.Completed => "bg-info",
            QuotationStatus.PartiallyApproved => "bg-warning",
            _ => "bg-secondary"
        };
    }

    public class QuotationDetailsViewModel
    {
        public int Id { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public int RfqId { get; set; }
        public string RfqNumber { get; set; } = string.Empty;
        public string RfqTitle { get; set; } = string.Empty;
        public string SupplierCompanyName { get; set; } = string.Empty;
        public QuotationStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? ValidUntil { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsOwner { get; set; }
        public bool IsRfqOwner { get; set; }
        public List<QuotationItemViewModel> Items { get; set; } = new();

        public string StatusText => Status switch
        {
            QuotationStatus.Draft => "Taslak",
            QuotationStatus.Submitted => "Gönderildi",
            QuotationStatus.Approved => "Onaylandı",
            QuotationStatus.Rejected => "Reddedildi",
            _ => "-"
        };
    }

    public class QuotationItemViewModel
    {
        public int Id { get; set; }
        public string RfqItemDescription { get; set; } = string.Empty;
        public decimal RequestedQuantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal OfferedQuantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public decimal? ApprovedQuantity { get; set; }
    }

    public class QuotationFormViewModel
    {
        public int Id { get; set; }
        public int RfqId { get; set; }
        public string RfqNumber { get; set; } = string.Empty;
        public string RfqTitle { get; set; } = string.Empty;
        public DateTime? RfqEndDate { get; set; }
        public string Currency { get; set; } = "TRY";
        public DateTime? ValidUntil { get; set; }
        public bool SubmitNow { get; set; }
        
        public string? ItemsJson { get; set; }
        
        public List<RfqItemForQuotationViewModel> RfqItems { get; set; } = new();
        public List<QuotationItemInputModel>? Items { get; set; }
    }

    public class RfqItemForQuotationViewModel
    {
        public int RfqItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? MaterialName { get; set; }
        public decimal RequestedQuantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string? TechnicalSpecs { get; set; }
    }

    public class QuotationItemInputModel
    {
        public int RfqItemId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OfferedQuantity { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }

    #endregion
}
