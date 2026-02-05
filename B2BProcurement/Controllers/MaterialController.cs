using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using B2BProcurement.Business.DTOs.Material;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Data.Context;
using System.Security.Claims;

namespace B2BProcurement.Controllers
{
    /// <summary>
    /// Malzeme yönetimi controller'ı.
    /// </summary>
    [Authorize]
    public class MaterialController : Controller
    {
        private readonly IMaterialService _materialService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MaterialController(
            IMaterialService materialService,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _materialService = materialService;
            _context = context;
            _environment = environment;
        }

        #region Index (List with Search, Filter, Pagination)

        /// <summary>
        /// Malzeme listesi sayfası.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? search, int? sectorId, bool? isActive, int page = 1)
        {
            var companyId = GetCurrentCompanyId();
            
            var query = _context.Materials
                .Include(m => m.Sector)
                .Include(m => m.Documents)
                .Where(m => m.CompanyId == companyId);

            // Arama
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(m => 
                    m.Name.ToLower().Contains(search) ||
                    m.Code.ToLower().Contains(search) ||
                    (m.Description != null && m.Description.ToLower().Contains(search)));
            }

            // Sektör filtresi
            if (sectorId.HasValue)
            {
                query = query.Where(m => m.SectorId == sectorId.Value);
            }

            // Durum filtresi
            if (isActive.HasValue)
            {
                query = query.Where(m => m.IsActive == isActive.Value);
            }

            // Sayfalama
            const int pageSize = 10;
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var materials = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MaterialListViewModel
                {
                    Id = m.Id,
                    Code = m.Code,
                    Name = m.Name,
                    Unit = m.Unit,
                    SectorName = m.Sector != null ? m.Sector.Name : "-",
                    IsActive = m.IsActive,
                    IsPublic = m.IsPublic,
                    DocumentCount = m.Documents.Count,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            await LoadFilterData();
            
            ViewBag.Search = search;
            ViewBag.SectorId = sectorId;
            ViewBag.IsActive = isActive;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(materials);
        }

        /// <summary>
        /// DataTables için AJAX endpoint.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetMaterialsData()
        {
            var companyId = GetCurrentCompanyId();
            
            // DataTables parametreleri
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = int.Parse(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = int.Parse(Request.Form["length"].FirstOrDefault() ?? "10");
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            var query = _context.Materials
                .Include(m => m.Sector)
                .Include(m => m.Documents)
                .Where(m => m.CompanyId == companyId);

            // Toplam kayıt sayısı
            var recordsTotal = await query.CountAsync();

            // Arama
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                query = query.Where(m =>
                    m.Name.ToLower().Contains(searchValue) ||
                    m.Code.ToLower().Contains(searchValue) ||
                    (m.Description != null && m.Description.ToLower().Contains(searchValue)));
            }

            var recordsFiltered = await query.CountAsync();

            // Sıralama
            query = sortColumn switch
            {
                "code" => sortDirection == "asc" ? query.OrderBy(m => m.Code) : query.OrderByDescending(m => m.Code),
                "name" => sortDirection == "asc" ? query.OrderBy(m => m.Name) : query.OrderByDescending(m => m.Name),
                "sector" => sortDirection == "asc" ? query.OrderBy(m => m.Sector!.Name) : query.OrderByDescending(m => m.Sector!.Name),
                "createdAt" => sortDirection == "asc" ? query.OrderBy(m => m.CreatedAt) : query.OrderByDescending(m => m.CreatedAt),
                _ => query.OrderByDescending(m => m.CreatedAt)
            };

            var data = await query
                .Skip(start)
                .Take(length)
                .Select(m => new
                {
                    m.Id,
                    m.Code,
                    m.Name,
                    m.Unit,
                    Sector = m.Sector != null ? m.Sector.Name : "-",
                    m.IsActive,
                    m.IsPublic,
                    DocumentCount = m.Documents.Count,
                    CreatedAt = m.CreatedAt.ToString("dd.MM.yyyy")
                })
                .ToListAsync();

            return Json(new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data
            });
        }

        #endregion

        #region Details

        /// <summary>
        /// Malzeme detay sayfası.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var companyId = GetCurrentCompanyId();
            
            var material = await _context.Materials
                .Include(m => m.Sector)
                .Include(m => m.Documents)
                .Include(m => m.RFQItems)
                    .ThenInclude(ri => ri.RFQ)
                .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId);

            if (material == null)
            {
                return NotFound();
            }

            var viewModel = new MaterialDetailsViewModel
            {
                Id = material.Id,
                Code = material.Code,
                Name = material.Name,
                Description = material.Description,
                Unit = material.Unit,
                SectorName = material.Sector?.Name ?? "-",
                IsActive = material.IsActive,
                IsPublic = material.IsPublic,
                CreatedAt = material.CreatedAt,
                UpdatedAt = material.UpdatedAt,
                Documents = material.Documents.Select(d => new MaterialDocumentViewModel
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    FilePath = d.FilePath,
                    FileSize = d.FileSize,
                    FileType = d.FileType,
                    UploadedAt = d.CreatedAt
                }).ToList(),
                RelatedRfqs = material.RFQItems.Select(ri => new RelatedRfqViewModel
                {
                    RfqId = ri.RFQ?.Id ?? 0,
                    RfqNumber = ri.RFQ?.RfqNumber ?? "-",
                    RfqTitle = ri.RFQ?.Title ?? "-",
                    Quantity = ri.Quantity,
                    Status = ri.RFQ?.Status.ToString() ?? "-"
                }).ToList()
            };

            return View(viewModel);
        }

        #endregion

        #region Create

        /// <summary>
        /// Malzeme oluşturma sayfası.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadFormData();
            return View(new MaterialFormViewModel());
        }

        /// <summary>
        /// Malzeme oluşturma işlemi.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaterialFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormData();
                return View(model);
            }

            var companyId = GetCurrentCompanyId();

            // Kod benzersizlik kontrolü
            if (!await _materialService.IsCodeUniqueAsync(companyId, model.Code))
            {
                ModelState.AddModelError("Code", "Bu malzeme kodu zaten kullanılıyor.");
                await LoadFormData();
                return View(model);
            }

            try
            {
                var dto = new MaterialCreateDto
                {
                    Code = model.Code,
                    Name = model.Name,
                    Description = model.Description,
                    Unit = model.Unit,
                    SectorId = model.SectorId,
                    IsPublic = model.IsPublic
                };

                var result = await _materialService.CreateAsync(companyId, dto);

                // Dosya yükleme
                if (model.Documents != null && model.Documents.Count > 0)
                {
                    await SaveDocumentsAsync(result.Id, model.Documents);
                }

                TempData["SuccessMessage"] = "Malzeme başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Details), new { id = result.Id });
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
        /// Malzeme düzenleme sayfası.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var companyId = GetCurrentCompanyId();
            
            var material = await _context.Materials
                .Include(m => m.Documents)
                .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId);

            if (material == null)
            {
                return NotFound();
            }

            var viewModel = new MaterialFormViewModel
            {
                Id = material.Id,
                Code = material.Code,
                Name = material.Name,
                Description = material.Description,
                Unit = material.Unit,
                SectorId = material.SectorId,
                IsPublic = material.IsPublic,
                IsActive = material.IsActive,
                ExistingDocuments = material.Documents.Select(d => new MaterialDocumentViewModel
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    FilePath = d.FilePath,
                    FileSize = d.FileSize,
                    FileType = d.FileType,
                    UploadedAt = d.CreatedAt
                }).ToList()
            };

            await LoadFormData();
            return View(viewModel);
        }

        /// <summary>
        /// Malzeme düzenleme işlemi.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaterialFormViewModel model)
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

            var companyId = GetCurrentCompanyId();

            // Kod benzersizlik kontrolü
            if (!await _materialService.IsCodeUniqueAsync(companyId, model.Code, id))
            {
                ModelState.AddModelError("Code", "Bu malzeme kodu zaten kullanılıyor.");
                await LoadFormData();
                return View(model);
            }

            try
            {
                var dto = new MaterialUpdateDto
                {
                    Code = model.Code,
                    Name = model.Name,
                    Description = model.Description,
                    Unit = model.Unit,
                    SectorId = model.SectorId,
                    IsPublic = model.IsPublic,
                    IsActive = model.IsActive
                };

                await _materialService.UpdateAsync(id, dto);

                // Yeni dosyaları yükle
                if (model.Documents != null && model.Documents.Count > 0)
                {
                    await SaveDocumentsAsync(id, model.Documents);
                }

                TempData["SuccessMessage"] = "Malzeme başarıyla güncellendi.";
                return RedirectToAction(nameof(Details), new { id });
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
        /// Malzeme silme (AJAX).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = GetCurrentCompanyId();
            
            var material = await _context.Materials
                .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId);

            if (material == null)
            {
                return Json(new { success = false, message = "Malzeme bulunamadı." });
            }

            try
            {
                await _materialService.DeleteAsync(id);
                return Json(new { success = true, message = "Malzeme başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Document Management

        /// <summary>
        /// Doküman yükleme (AJAX).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UploadDocument(int materialId, IFormFile file)
        {
            var companyId = GetCurrentCompanyId();
            
            var material = await _context.Materials
                .FirstOrDefaultAsync(m => m.Id == materialId && m.CompanyId == companyId);

            if (material == null)
            {
                return Json(new { success = false, message = "Malzeme bulunamadı." });
            }

            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Dosya seçilmedi." });
            }

            try
            {
                var document = await SaveSingleDocumentAsync(materialId, file);
                return Json(new 
                { 
                    success = true, 
                    message = "Dosya yüklendi.",
                    document = new
                    {
                        id = document.Id,
                        fileName = document.FileName,
                        fileSize = FormatFileSize(document.FileSize),
                        fileType = document.FileType
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Doküman silme (AJAX).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDocument(int documentId)
        {
            var companyId = GetCurrentCompanyId();
            
            var document = await _context.MaterialDocuments
                .Include(d => d.Material)
                .FirstOrDefaultAsync(d => d.Id == documentId && d.Material!.CompanyId == companyId);

            if (document == null)
            {
                return Json(new { success = false, message = "Doküman bulunamadı." });
            }

            try
            {
                // Fiziksel dosyayı sil
                var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.MaterialDocuments.Remove(document);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Doküman silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Excel Export

        /// <summary>
        /// Excel export.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            var companyId = GetCurrentCompanyId();
            
            var materials = await _context.Materials
                .Include(m => m.Sector)
                .Where(m => m.CompanyId == companyId)
                .OrderBy(m => m.Code)
                .ToListAsync();

            // CSV formatında export (basit implementasyon)
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Kod,Ad,Açıklama,Birim,Sektör,Durum,Herkese Açık,Oluşturma Tarihi");

            foreach (var m in materials)
            {
                csv.AppendLine($"\"{m.Code}\",\"{m.Name}\",\"{m.Description ?? ""}\",\"{m.Unit}\",\"{m.Sector?.Name ?? "-"}\",\"{(m.IsActive ? "Aktif" : "Pasif")}\",\"{(m.IsPublic ? "Evet" : "Hayır")}\",\"{m.CreatedAt:dd.MM.yyyy}\"");
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
            return File(bytes, "text/csv", $"Malzemeler_{DateTime.Now:yyyyMMdd}.csv");
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
            ViewBag.Units = GetUnitList();
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

        private List<SelectListItem> GetUnitList()
        {
            return new List<SelectListItem>
            {
                new("Adet", "Adet"),
                new("Kg", "Kg"),
                new("Gram", "Gram"),
                new("Litre", "Litre"),
                new("Metre", "Metre"),
                new("Metrekare", "Metrekare"),
                new("Paket", "Paket"),
                new("Kutu", "Kutu"),
                new("Set", "Set"),
                new("Ton", "Ton")
            };
        }

        private async Task SaveDocumentsAsync(int materialId, List<IFormFile> files)
        {
            foreach (var file in files)
            {
                await SaveSingleDocumentAsync(materialId, file);
            }
        }

        private async Task<B2BProcurement.Core.Entities.MaterialDocument> SaveSingleDocumentAsync(int materialId, IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "materials", materialId.ToString());
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var document = new B2BProcurement.Core.Entities.MaterialDocument
            {
                MaterialId = materialId,
                FileName = file.FileName,
                FilePath = $"/uploads/materials/{materialId}/{uniqueFileName}",
                FileSize = file.Length,
                FileType = file.ContentType
            };

            _context.MaterialDocuments.Add(document);
            await _context.SaveChangesAsync();

            return document;
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }

        #endregion
    }

    #region ViewModels

    public class MaterialListViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string SectorName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsPublic { get; set; }
        public int DocumentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MaterialDetailsViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string SectorName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<MaterialDocumentViewModel> Documents { get; set; } = new();
        public List<RelatedRfqViewModel> RelatedRfqs { get; set; } = new();
    }

    public class MaterialFormViewModel
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Malzeme kodu zorunludur.")]
        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Malzeme Kodu")]
        public string Code { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Malzeme adı zorunludur.")]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Malzeme Adı")]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.MaxLength(1000)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Birim zorunludur.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Birim")]
        public string Unit { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Sektör zorunludur.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Sektör")]
        public int SectorId { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Herkese Açık")]
        public bool IsPublic { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        public List<IFormFile>? Documents { get; set; }
        public List<MaterialDocumentViewModel> ExistingDocuments { get; set; } = new();
    }

    public class MaterialDocumentViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }

        public string FormattedSize
        {
            get
            {
                string[] sizes = { "B", "KB", "MB", "GB" };
                int order = 0;
                double size = FileSize;
                while (size >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    size /= 1024;
                }
                return $"{size:0.##} {sizes[order]}";
            }
        }

        public bool IsImage => FileType.StartsWith("image/");
    }

    public class RelatedRfqViewModel
    {
        public int RfqId { get; set; }
        public string RfqNumber { get; set; } = string.Empty;
        public string RfqTitle { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    #endregion
}
