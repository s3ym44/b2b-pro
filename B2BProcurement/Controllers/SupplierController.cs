using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using B2BProcurement.Business.DTOs.Supplier;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Data.Context;
using System.Security.Claims;

namespace B2BProcurement.Controllers
{
    /// <summary>
    /// Tedarikçi yönetimi controller'ı.
    /// </summary>
    [Authorize]
    public class SupplierController : Controller
    {
        private readonly ISupplierService _supplierService;
        private readonly ApplicationDbContext _context;

        public SupplierController(
            ISupplierService supplierService,
            ApplicationDbContext context)
        {
            _supplierService = supplierService;
            _context = context;
        }

        #region Index

        /// <summary>
        /// Tedarikçi listesi.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? search, int? sectorId, bool? isPlatformMember, int page = 1)
        {
            var companyId = GetCurrentCompanyId();
            
            var query = _context.Suppliers
                .Include(s => s.Sector)
                .Include(s => s.SupplierCompany)
                .Where(s => s.CompanyId == companyId && s.IsActive);

            // Arama
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(s => 
                    s.Name.ToLower().Contains(search) ||
                    (s.TaxNumber != null && s.TaxNumber.Contains(search)) ||
                    (s.Email != null && s.Email.ToLower().Contains(search)));
            }

            // Sektör filtresi
            if (sectorId.HasValue)
            {
                query = query.Where(s => s.SectorId == sectorId.Value);
            }

            // Platform üyesi filtresi
            if (isPlatformMember.HasValue)
            {
                query = isPlatformMember.Value 
                    ? query.Where(s => s.SupplierCompanyId != null)
                    : query.Where(s => s.SupplierCompanyId == null);
            }

            // Sayfalama
            const int pageSize = 10;
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var suppliers = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SupplierListViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    TaxNumber = s.TaxNumber,
                    Email = s.Email,
                    Phone = s.Phone,
                    SectorName = s.Sector != null ? s.Sector.Name : "-",
                    IsPlatformMember = s.SupplierCompanyId != null,
                    SupplierCompanyName = s.SupplierCompany != null ? s.SupplierCompany.CompanyName : null,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            await LoadFilterData();
            
            ViewBag.Search = search;
            ViewBag.SectorId = sectorId;
            ViewBag.IsPlatformMember = isPlatformMember;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(suppliers);
        }

        /// <summary>
        /// DataTables AJAX endpoint.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetSuppliersData()
        {
            var companyId = GetCurrentCompanyId();
            
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = int.Parse(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = int.Parse(Request.Form["length"].FirstOrDefault() ?? "10");
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            var query = _context.Suppliers
                .Include(s => s.Sector)
                .Include(s => s.SupplierCompany)
                .Where(s => s.CompanyId == companyId && s.IsActive);

            var recordsTotal = await query.CountAsync();

            // Arama
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(searchValue) ||
                    (s.TaxNumber != null && s.TaxNumber.Contains(searchValue)) ||
                    (s.Email != null && s.Email.ToLower().Contains(searchValue)));
            }

            var recordsFiltered = await query.CountAsync();

            // Sıralama
            query = sortColumn switch
            {
                "name" => sortDirection == "asc" ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
                "sector" => sortDirection == "asc" ? query.OrderBy(s => s.Sector!.Name) : query.OrderByDescending(s => s.Sector!.Name),
                "createdAt" => sortDirection == "asc" ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt),
                _ => query.OrderByDescending(s => s.CreatedAt)
            };

            var data = await query
                .Skip(start)
                .Take(length)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.TaxNumber,
                    s.Email,
                    s.Phone,
                    Sector = s.Sector != null ? s.Sector.Name : "-",
                    IsPlatformMember = s.SupplierCompanyId != null,
                    SupplierCompanyName = s.SupplierCompany != null ? s.SupplierCompany.CompanyName : null,
                    CreatedAt = s.CreatedAt.ToString("dd.MM.yyyy")
                })
                .ToListAsync();

            return Json(new { draw, recordsTotal, recordsFiltered, data });
        }

        #endregion

        #region Create

        /// <summary>
        /// Tedarikçi oluşturma sayfası.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadFormData();
            return View(new SupplierFormViewModel());
        }

        /// <summary>
        /// Tedarikçi oluşturma işlemi.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormData();
                return View(model);
            }

            var companyId = GetCurrentCompanyId();

            try
            {
                var dto = new SupplierCreateDto
                {
                    Name = model.Name,
                    TaxNumber = model.TaxNumber,
                    Email = model.Email,
                    Phone = model.Phone,
                    SectorId = model.SectorId,
                    SupplierCompanyId = model.SupplierCompanyId
                };

                var result = await _supplierService.CreateAsync(companyId, dto);

                TempData["SuccessMessage"] = "Tedarikçi başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
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
        /// Tedarikçi düzenleme sayfası.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var companyId = GetCurrentCompanyId();
            
            var supplier = await _context.Suppliers
                .Include(s => s.SupplierCompany)
                .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId);

            if (supplier == null)
            {
                return NotFound();
            }

            var viewModel = new SupplierFormViewModel
            {
                Id = supplier.Id,
                Name = supplier.Name,
                TaxNumber = supplier.TaxNumber,
                Email = supplier.Email,
                Phone = supplier.Phone,
                SectorId = supplier.SectorId,
                SupplierCompanyId = supplier.SupplierCompanyId,
                SupplierCompanyName = supplier.SupplierCompany?.CompanyName,
                IsPlatformMember = supplier.SupplierCompanyId != null
            };

            await LoadFormData();
            return View(viewModel);
        }

        /// <summary>
        /// Tedarikçi düzenleme işlemi.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierFormViewModel model)
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
                var dto = new SupplierUpdateDto
                {
                    Name = model.Name,
                    TaxNumber = model.TaxNumber,
                    Email = model.Email,
                    Phone = model.Phone,
                    SectorId = model.SectorId,
                    SupplierCompanyId = model.SupplierCompanyId
                };

                await _supplierService.UpdateAsync(id, dto);

                TempData["SuccessMessage"] = "Tedarikçi başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadFormData();
                return View(model);
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// Tedarikçi silme (AJAX).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = GetCurrentCompanyId();
            
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId);

            if (supplier == null)
            {
                return Json(new { success = false, message = "Tedarikçi bulunamadı." });
            }

            try
            {
                await _supplierService.DeleteAsync(id);
                return Json(new { success = true, message = "Tedarikçi başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Search (Autocomplete & Platform Member Lookup)

        /// <summary>
        /// Tedarikçi arama (autocomplete).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(new List<object>());
            }

            var companyId = GetCurrentCompanyId();
            term = term.ToLower();

            var suppliers = await _context.Suppliers
                .Where(s => s.CompanyId == companyId && s.IsActive &&
                    (s.Name.ToLower().Contains(term) ||
                     (s.TaxNumber != null && s.TaxNumber.Contains(term))))
                .Take(10)
                .Select(s => new
                {
                    id = s.Id,
                    text = s.Name,
                    taxNumber = s.TaxNumber
                })
                .ToListAsync();

            return Json(suppliers);
        }

        /// <summary>
        /// Platform üyesi arama (vergi no ile).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchPlatformMember(string taxNumber)
        {
            if (string.IsNullOrWhiteSpace(taxNumber))
            {
                return Json(new { found = false });
            }

            var currentCompanyId = GetCurrentCompanyId();

            // Vergi numarası ile platform üyesi ara
            var company = await _context.Companies
                .Include(c => c.Sector)
                .FirstOrDefaultAsync(c => c.TaxNumber == taxNumber && c.Id != currentCompanyId);

            if (company == null)
            {
                return Json(new { found = false, message = "Bu vergi numarası ile kayıtlı platform üyesi bulunamadı." });
            }

            return Json(new
            {
                found = true,
                companyId = company.Id,
                companyName = company.CompanyName,
                sectorId = company.SectorId,
                sectorName = company.Sector?.Name,
                email = company.Email,
                phone = company.Phone,
                city = company.City
            });
        }

        #endregion

        #region Helper Methods

        private int GetCurrentCompanyId()
        {
            var claim = User.FindFirst("CompanyId");
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        private async Task LoadFormData()
        {
            var sectors = await _context.Sectors
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            ViewBag.Sectors = sectors;
        }

        private async Task LoadFilterData()
        {
            var sectors = await _context.Sectors
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            ViewBag.Sectors = sectors;
        }

        #endregion
    }

    #region ViewModels

    public class SupplierListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? TaxNumber { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string SectorName { get; set; } = string.Empty;
        public bool IsPlatformMember { get; set; }
        public string? SupplierCompanyName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SupplierFormViewModel
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Tedarikçi adı zorunludur.")]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Tedarikçi Adı")]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.StringLength(11, MinimumLength = 10, ErrorMessage = "Vergi numarası 10 veya 11 haneli olmalıdır.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Vergi Numarası")]
        public string? TaxNumber { get; set; }

        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "E-posta")]
        public string? Email { get; set; }

        [System.ComponentModel.DataAnnotations.Phone(ErrorMessage = "Geçerli bir telefon numarası girin.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Sektör zorunludur.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Sektör")]
        public int SectorId { get; set; }

        // Platform üyesi bağlantısı
        public int? SupplierCompanyId { get; set; }
        public string? SupplierCompanyName { get; set; }
        public bool IsPlatformMember { get; set; }
    }

    #endregion
}
