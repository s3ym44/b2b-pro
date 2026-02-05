using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using B2BProcurement.Business.DTOs.Rfq;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Core.Enums;
using B2BProcurement.Data.Context;
using System.Security.Claims;

namespace B2BProcurement.Controllers
{
    /// <summary>
    /// RFQ (Teklif Talebi) yönetimi controller'ı.
    /// </summary>
    [Authorize]
    public class RfqController : Controller
    {
        private readonly IRfqService _rfqService;
        private readonly ApplicationDbContext _context;

        public RfqController(IRfqService rfqService, ApplicationDbContext context)
        {
            _rfqService = rfqService;
            _context = context;
        }

        #region Index - My RFQs List

        /// <summary>
        /// Oluşturduğum RFQ listesi.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(RfqStatus? status, int page = 1)
        {
            var companyId = GetCurrentCompanyId();
            
            var query = _context.RFQs
                .Include(r => r.Sector)
                .Include(r => r.Items)
                .Include(r => r.Quotations)
                .Where(r => r.CompanyId == companyId);

            // Durum filtresi
            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            // Sayfalama
            const int pageSize = 10;
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var rfqs = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RfqListViewModel
                {
                    Id = r.Id,
                    RfqNumber = r.RfqNumber,
                    Title = r.Title,
                    SectorName = r.Sector != null ? r.Sector.Name : "-",
                    Status = r.Status,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    ItemCount = r.Items.Count,
                    QuotationCount = r.Quotations.Count,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.Statuses = GetStatusList();

            return View(rfqs);
        }

        #endregion

        #region Incoming - RFQs for My Sector

        /// <summary>
        /// Sektörüme gelen RFQ listesi.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Incoming(int page = 1)
        {
            var companyId = GetCurrentCompanyId();
            var company = await _context.Companies.FindAsync(companyId);
            
            if (company == null) return NotFound();

            var now = DateTime.Now;
            var query = _context.RFQs
                .Include(r => r.Company)
                .Include(r => r.Sector)
                .Include(r => r.Items)
                .Include(r => r.Quotations)
                .Where(r => r.Status == RfqStatus.Published &&
                           r.EndDate > now &&
                           r.SectorId == company.SectorId &&
                           r.CompanyId != companyId);

            // Sayfalama
            const int pageSize = 10;
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var rfqs = await query
                .OrderBy(r => r.EndDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RfqIncomingViewModel
                {
                    Id = r.Id,
                    RfqNumber = r.RfqNumber,
                    Title = r.Title,
                    CompanyName = r.Company != null ? r.Company.CompanyName : "-",
                    SectorName = r.Sector != null ? r.Sector.Name : "-",
                    EndDate = r.EndDate,
                    ItemCount = r.Items.Count,
                    QuotationCount = r.Quotations.Count,
                    HasMyQuotation = r.Quotations.Any(q => q.SupplierCompanyId == companyId),
                    DaysRemaining = (int)(r.EndDate - now).TotalDays
                })
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(rfqs);
        }

        #endregion

        #region Details

        /// <summary>
        /// RFQ detay sayfası.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var companyId = GetCurrentCompanyId();
            
            var rfq = await _context.RFQs
                .Include(r => r.Company)
                .Include(r => r.Sector)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Material)
                .Include(r => r.Documents)
                .Include(r => r.Quotations)
                    .ThenInclude(q => q.SupplierCompany)
                .Include(r => r.Quotations)
                    .ThenInclude(q => q.Items)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rfq == null) return NotFound();

            // Erişim kontrolü
            var isOwner = rfq.CompanyId == companyId;
            var isSupplier = rfq.Status == RfqStatus.Published && rfq.SectorId == await GetCompanySectorIdAsync(companyId);

            if (!isOwner && !isSupplier) return Forbid();

            var viewModel = new RfqDetailsViewModel
            {
                Id = rfq.Id,
                RfqNumber = rfq.RfqNumber,
                Title = rfq.Title,
                CompanyName = rfq.Company?.CompanyName ?? "-",
                SectorName = rfq.Sector?.Name ?? "-",
                Status = rfq.Status,
                Visibility = rfq.Visibility,
                StartDate = rfq.StartDate,
                EndDate = rfq.EndDate,
                Currency = rfq.Currency,
                CreatedAt = rfq.CreatedAt,
                IsOwner = isOwner,
                Items = rfq.Items.Select(i => new RfqItemViewModel
                {
                    Id = i.Id,
                    MaterialName = i.Material?.Name,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    TechnicalSpecs = i.TechnicalSpecs,
                    DeliveryDate = i.DeliveryDate
                }).ToList(),
                Quotations = rfq.Quotations.Select(q => new RfqQuotationViewModel
                {
                    Id = q.Id,
                    QuotationNumber = q.QuotationNumber,
                    SupplierName = q.SupplierCompany?.CompanyName ?? "-",
                    Status = q.Status,
                    TotalAmount = q.TotalAmount,
                    ValidUntil = q.ValidUntil,
                    ItemCount = q.Items.Count,
                    CreatedAt = q.CreatedAt
                }).ToList()
            };

            return View(viewModel);
        }

        #endregion

        #region Create

        /// <summary>
        /// RFQ oluşturma sayfası.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new RfqFormViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(14),
                Currency = "TRY"
            };
            
            await LoadFormData();
            return View(model);
        }

        /// <summary>
        /// RFQ oluşturma işlemi.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RfqFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormData();
                return View(model);
            }

            var companyId = GetCurrentCompanyId();

            try
            {
                var dto = new RfqCreateDto
                {
                    Title = model.Title,
                    SectorId = model.SectorId,
                    Visibility = model.Visibility,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Currency = model.Currency
                };

                var result = await _rfqService.CreateAsync(companyId, dto);

                TempData["SuccessMessage"] = "RFQ taslağı oluşturuldu. Şimdi kalemler ekleyebilirsiniz.";
                return RedirectToAction(nameof(Edit), new { id = result.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadFormData();
                return View(model);
            }
        }

        #endregion

        #region Edit

        /// <summary>
        /// RFQ düzenleme sayfası.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var companyId = GetCurrentCompanyId();
            
            var rfq = await _context.RFQs
                .Include(r => r.Items)
                    .ThenInclude(i => i.Material)
                .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

            if (rfq == null) return NotFound();

            if (rfq.Status != RfqStatus.Draft)
            {
                TempData["ErrorMessage"] = "Sadece taslak durumundaki RFQ'lar düzenlenebilir.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var model = new RfqFormViewModel
            {
                Id = rfq.Id,
                RfqNumber = rfq.RfqNumber,
                Title = rfq.Title,
                SectorId = rfq.SectorId,
                Visibility = rfq.Visibility,
                StartDate = rfq.StartDate,
                EndDate = rfq.EndDate,
                Currency = rfq.Currency,
                Items = rfq.Items.Select(i => new RfqItemViewModel
                {
                    Id = i.Id,
                    MaterialId = i.MaterialId,
                    MaterialName = i.Material?.Name,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    TechnicalSpecs = i.TechnicalSpecs,
                    DeliveryDate = i.DeliveryDate
                }).ToList()
            };

            await LoadFormData();
            return View(model);
        }

        /// <summary>
        /// RFQ düzenleme işlemi.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RfqFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await LoadFormData();
                return View(model);
            }

            try
            {
                var dto = new RfqUpdateDto
                {
                    Title = model.Title,
                    SectorId = model.SectorId,
                    Visibility = model.Visibility,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Currency = model.Currency
                };

                await _rfqService.UpdateAsync(id, dto);

                TempData["SuccessMessage"] = "RFQ başarıyla güncellendi.";
                return RedirectToAction(nameof(Edit), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadFormData();
                return View(model);
            }
        }

        #endregion

        #region Status Actions (Publish, Close, Cancel)

        /// <summary>
        /// RFQ yayınla.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var companyId = GetCurrentCompanyId();
            var rfq = await _context.RFQs
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

            if (rfq == null) return NotFound();

            if (rfq.Status != RfqStatus.Draft)
            {
                return Json(new { success = false, message = "Sadece taslak RFQ'lar yayınlanabilir." });
            }

            if (!rfq.Items.Any())
            {
                return Json(new { success = false, message = "RFQ yayınlamak için en az bir kalem eklemelisiniz." });
            }

            try
            {
                await _rfqService.PublishAsync(id);
                return Json(new { success = true, message = "RFQ başarıyla yayınlandı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// RFQ kapat.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            var companyId = GetCurrentCompanyId();
            var rfq = await _context.RFQs.FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

            if (rfq == null) return NotFound();

            if (rfq.Status != RfqStatus.Published)
            {
                return Json(new { success = false, message = "Sadece yayınlanmış RFQ'lar kapatılabilir." });
            }

            try
            {
                await _rfqService.CloseAsync(id);
                return Json(new { success = true, message = "RFQ başarıyla kapatıldı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// RFQ iptal et.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var companyId = GetCurrentCompanyId();
            var rfq = await _context.RFQs.FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

            if (rfq == null) return NotFound();

            try
            {
                await _rfqService.CancelAsync(id);
                return Json(new { success = true, message = "RFQ iptal edildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Item Management (AJAX)

        /// <summary>
        /// Kalem ekle.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddItem(int rfqId, [FromBody] RfqItemFormModel item)
        {
            var companyId = GetCurrentCompanyId();
            var rfq = await _context.RFQs.FirstOrDefaultAsync(r => r.Id == rfqId && r.CompanyId == companyId);

            if (rfq == null || rfq.Status != RfqStatus.Draft)
            {
                return Json(new { success = false, message = "Kalem eklenemez." });
            }

            try
            {
                var dto = new RfqItemCreateDto
                {
                    MaterialId = item.MaterialId,
                    Description = item.Description ?? string.Empty,
                    Quantity = item.Quantity,
                    Unit = item.Unit ?? "Adet",
                    TechnicalSpecs = item.TechnicalSpecs,
                    //DeliveryDate will be added later if needed
                };

                await _rfqService.AddItemAsync(rfqId, dto);
                
                // Güncel kalemleri döndür
                var items = await GetRfqItemsAsync(rfqId);
                return Json(new { success = true, message = "Kalem eklendi.", items });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Kalem sil.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var companyId = GetCurrentCompanyId();
            var item = await _context.RFQItems
                .Include(i => i.RFQ)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.RFQ!.CompanyId == companyId);

            if (item == null || item.RFQ?.Status != RfqStatus.Draft)
            {
                return Json(new { success = false, message = "Kalem silinemez." });
            }

            try
            {
                await _rfqService.DeleteItemAsync(itemId);
                return Json(new { success = true, message = "Kalem silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Comparison Matrix

        /// <summary>
        /// Teklif karşılaştırma matrisi.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Comparison(int id)
        {
            var companyId = GetCurrentCompanyId();
            
            var rfq = await _context.RFQs
                .Include(r => r.Items)
                    .ThenInclude(i => i.Material)
                .Include(r => r.Quotations.Where(q => q.Status == QuotationStatus.Submitted || q.Status == QuotationStatus.Approved))
                    .ThenInclude(q => q.SupplierCompany)
                .Include(r => r.Quotations)
                    .ThenInclude(q => q.Items)
                .FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId);

            if (rfq == null) return NotFound();

            var viewModel = new RfqComparisonViewModel
            {
                RfqId = rfq.Id,
                RfqNumber = rfq.RfqNumber,
                Title = rfq.Title,
                Currency = rfq.Currency,
                Items = rfq.Items.Select(i => new ComparisonItemViewModel
                {
                    ItemId = i.Id,
                    Description = i.Description,
                    MaterialName = i.Material?.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit
                }).ToList(),
                Suppliers = rfq.Quotations
                    .Where(q => q.Status == QuotationStatus.Submitted || q.Status == QuotationStatus.Approved)
                    .Select(q => new ComparisonSupplierViewModel
                    {
                        QuotationId = q.Id,
                        SupplierName = q.SupplierCompany?.CompanyName ?? "-",
                        TotalAmount = q.TotalAmount,
                        Status = q.Status,
                        ValidUntil = q.ValidUntil,
                        ItemPrices = q.Items.ToDictionary(
                            qi => qi.RfqItemId,
                            qi => new ComparisonPriceViewModel
                            {
                                UnitPrice = qi.UnitPrice,
                                TotalPrice = qi.TotalPrice,
                                DeliveryDays = null // DeliveryDate exists but not DeliveryDays
                            })
                    }).ToList()
            };

            return View(viewModel);
        }

        #endregion

        #region Helper Methods

        private int GetCurrentCompanyId()
        {
            var claim = User.FindFirst("CompanyId");
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        private async Task<int> GetCompanySectorIdAsync(int companyId)
        {
            var company = await _context.Companies.FindAsync(companyId);
            return company?.SectorId ?? 0;
        }

        private async Task LoadFormData()
        {
            var sectors = await _context.Sectors
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            var materials = await _context.Materials
                .Where(m => m.CompanyId == GetCurrentCompanyId() && m.IsActive)
                .OrderBy(m => m.Name)
                .Select(m => new { m.Id, m.Name, m.Code, m.Unit })
                .ToListAsync();

            ViewBag.Sectors = sectors;
            ViewBag.Materials = materials;
            ViewBag.Visibilities = GetVisibilityList();
            ViewBag.Currencies = GetCurrencyList();
        }

        private List<SelectListItem> GetStatusList()
        {
            return new List<SelectListItem>
            {
                new("Tümü", ""),
                new("Taslak", ((int)RfqStatus.Draft).ToString()),
                new("Yayınlandı", ((int)RfqStatus.Published).ToString()),
                new("Kapalı", ((int)RfqStatus.Closed).ToString()),
                new("İptal", ((int)RfqStatus.Cancelled).ToString())
            };
        }

        private List<SelectListItem> GetVisibilityList()
        {
            return new List<SelectListItem>
            {
                new("Kendi Tedarikcilerim", ((int)RfqVisibility.MySuppliers).ToString()),
                new("Tüm Sektör", ((int)RfqVisibility.AllSector).ToString()),
                new("Seçili Tedarikciler", ((int)RfqVisibility.Selected).ToString())
            };
        }

        private List<SelectListItem> GetCurrencyList()
        {
            return new List<SelectListItem>
            {
                new("TRY - Türk Lirası", "TRY"),
                new("USD - Amerikan Doları", "USD"),
                new("EUR - Euro", "EUR")
            };
        }

        private async Task<List<object>> GetRfqItemsAsync(int rfqId)
        {
            return await _context.RFQItems
                .Include(i => i.Material)
                .Where(i => i.RfqId == rfqId)
                .Select(i => new
                {
                    id = i.Id,
                    materialName = i.Material != null ? i.Material.Name : null,
                    description = i.Description,
                    quantity = i.Quantity,
                    unit = i.Unit,
                    deliveryDate = i.DeliveryDate
                })
                .ToListAsync<object>();
        }

        #endregion
    }

    #region ViewModels

    public class RfqListViewModel
    {
        public int Id { get; set; }
        public string RfqNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string SectorName { get; set; } = string.Empty;
        public RfqStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ItemCount { get; set; }
        public int QuotationCount { get; set; }
        public DateTime CreatedAt { get; set; }

        public string StatusBadgeClass => Status switch
        {
            RfqStatus.Draft => "bg-secondary",
            RfqStatus.Published => "bg-success",
            RfqStatus.Closed => "bg-info",
            RfqStatus.Cancelled => "bg-danger",
            _ => "bg-secondary"
        };

        public string StatusText => Status switch
        {
            RfqStatus.Draft => "Taslak",
            RfqStatus.Published => "Yayında",
            RfqStatus.Closed => "Kapalı",
            RfqStatus.Cancelled => "İptal",
            _ => "-"
        };
    }

    public class RfqIncomingViewModel
    {
        public int Id { get; set; }
        public string RfqNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string SectorName { get; set; } = string.Empty;
        public DateTime EndDate { get; set; }
        public int ItemCount { get; set; }
        public int QuotationCount { get; set; }
        public bool HasMyQuotation { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class RfqDetailsViewModel
    {
        public int Id { get; set; }
        public string RfqNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string SectorName { get; set; } = string.Empty;
        public RfqStatus Status { get; set; }
        public RfqVisibility Visibility { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsOwner { get; set; }
        public List<RfqItemViewModel> Items { get; set; } = new();
        public List<RfqQuotationViewModel> Quotations { get; set; } = new();
    }

    public class RfqFormViewModel
    {
        public int Id { get; set; }
        public string? RfqNumber { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Başlık zorunludur.")]
        [System.ComponentModel.DataAnnotations.MaxLength(300)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Başlık")]
        public string Title { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Sektör zorunludur.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Sektör")]
        public int SectorId { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Görünürlük")]
        public RfqVisibility Visibility { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "Başlangıç Tarihi")]
        public DateTime StartDate { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "Bitiş Tarihi")]
        public DateTime EndDate { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Para Birimi")]
        public string Currency { get; set; } = "TRY";

        public List<RfqItemViewModel> Items { get; set; } = new();
    }

    public class RfqItemViewModel
    {
        public int Id { get; set; }
        public int? MaterialId { get; set; }
        public string? MaterialName { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string? TechnicalSpecs { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }

    public class RfqItemFormModel
    {
        public int? MaterialId { get; set; }
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public string? TechnicalSpecs { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }

    public class RfqQuotationViewModel
    {
        public int Id { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public QuotationStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RfqComparisonViewModel
    {
        public int RfqId { get; set; }
        public string RfqNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public List<ComparisonItemViewModel> Items { get; set; } = new();
        public List<ComparisonSupplierViewModel> Suppliers { get; set; } = new();
    }

    public class ComparisonItemViewModel
    {
        public int ItemId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? MaterialName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    public class ComparisonSupplierViewModel
    {
        public int QuotationId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public QuotationStatus Status { get; set; }
        public DateTime? ValidUntil { get; set; }
        public Dictionary<int, ComparisonPriceViewModel> ItemPrices { get; set; } = new();
    }

    public class ComparisonPriceViewModel
    {
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int? DeliveryDays { get; set; }
    }

    #endregion
}
