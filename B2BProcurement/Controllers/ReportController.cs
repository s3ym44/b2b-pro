using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using B2BProcurement.Core.Enums;
using B2BProcurement.Data.Context;
using System.Security.Claims;

namespace B2BProcurement.Controllers
{
    /// <summary>
    /// Raporlama controller'ı.
    /// RFQ, Teklif, Tedarikçi ve Malzeme raporları.
    /// </summary>
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Helper Methods

        private int GetCurrentCompanyId()
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            return int.TryParse(companyIdClaim, out var companyId) ? companyId : 0;
        }

        private async Task<List<SelectListItem>> GetSectorsAsync()
        {
            return await _context.Sectors
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();
        }

        #endregion

        #region RFQ Report

        /// <summary>
        /// RFQ raporları sayfası.
        /// </summary>
        public async Task<IActionResult> RfqReport(RfqReportFilter filter)
        {
            var companyId = GetCurrentCompanyId();
            
            // Default date range: last 3 months
            filter.StartDate ??= DateTime.Now.AddMonths(-3);
            filter.EndDate ??= DateTime.Now;
            
            var query = _context.RFQs
                .Include(r => r.Sector)
                .Where(r => r.CompanyId == companyId && r.IsActive);
            
            // Apply filters
            if (filter.StartDate.HasValue)
                query = query.Where(r => r.CreatedAt >= filter.StartDate.Value);
            
            if (filter.EndDate.HasValue)
                query = query.Where(r => r.CreatedAt <= filter.EndDate.Value.AddDays(1));
            
            if (filter.Status.HasValue)
                query = query.Where(r => r.Status == filter.Status.Value);
            
            if (filter.SectorId.HasValue)
                query = query.Where(r => r.SectorId == filter.SectorId.Value);
            
            var rfqs = await query.ToListAsync();
            
            // Calculate statistics
            var totalRfqs = rfqs.Count;
            var draftCount = rfqs.Count(r => r.Status == RfqStatus.Draft);
            var publishedCount = rfqs.Count(r => r.Status == RfqStatus.Published);
            var closedCount = rfqs.Count(r => r.Status == RfqStatus.Closed);
            var cancelledCount = rfqs.Count(r => r.Status == RfqStatus.Cancelled);
            
            // Quotation counts
            var rfqIds = rfqs.Select(r => r.Id).ToList();
            var quotations = await _context.Quotations
                .Where(q => rfqIds.Contains(q.RfqId) && q.IsActive)
                .ToListAsync();
            
            var totalQuotations = quotations.Count;
            var avgQuotationsPerRfq = totalRfqs > 0 ? (double)totalQuotations / totalRfqs : 0;
            
            // Monthly data for chart
            var monthlyData = rfqs
                .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyDataPoint
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count()
                })
                .ToList();
            
            // Sector distribution
            var sectorData = rfqs
                .Where(r => r.Sector != null)
                .GroupBy(r => r.Sector!.Name)
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key,
                    Value = g.Count()
                })
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToList();
            
            var viewModel = new RfqReportViewModel
            {
                Filter = filter,
                Sectors = await GetSectorsAsync(),
                TotalRfqs = totalRfqs,
                DraftCount = draftCount,
                PublishedCount = publishedCount,
                ClosedCount = closedCount,
                CancelledCount = cancelledCount,
                TotalQuotations = totalQuotations,
                AvgQuotationsPerRfq = avgQuotationsPerRfq,
                MonthlyData = monthlyData,
                SectorDistribution = sectorData
            };
            
            return View(viewModel);
        }

        /// <summary>
        /// RFQ raporu Excel export.
        /// </summary>
        public async Task<IActionResult> ExportRfqReport(RfqReportFilter filter)
        {
            var companyId = GetCurrentCompanyId();
            
            var query = _context.RFQs
                .Include(r => r.Sector)
                .Where(r => r.CompanyId == companyId && r.IsActive);
            
            if (filter.StartDate.HasValue)
                query = query.Where(r => r.CreatedAt >= filter.StartDate.Value);
            
            if (filter.EndDate.HasValue)
                query = query.Where(r => r.CreatedAt <= filter.EndDate.Value.AddDays(1));
            
            if (filter.Status.HasValue)
                query = query.Where(r => r.Status == filter.Status.Value);
            
            if (filter.SectorId.HasValue)
                query = query.Where(r => r.SectorId == filter.SectorId.Value);
            
            var rfqs = await query
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.RfqNumber,
                    r.Title,
                    Sector = r.Sector != null ? r.Sector.Name : "",
                    r.Status,
                    r.StartDate,
                    r.EndDate,
                    r.CreatedAt
                })
                .ToListAsync();
            
            // Build CSV
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("RFQ No,Başlık,Sektör,Durum,Başlangıç,Bitiş,Oluşturulma");
            
            foreach (var rfq in rfqs)
            {
                csv.AppendLine($"{rfq.RfqNumber},{rfq.Title},{rfq.Sector},{rfq.Status},{rfq.StartDate:dd.MM.yyyy},{rfq.EndDate:dd.MM.yyyy},{rfq.CreatedAt:dd.MM.yyyy}");
            }
            
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"rfq_report_{DateTime.Now:yyyyMMdd}.csv");
        }

        #endregion

        #region Quotation Report

        /// <summary>
        /// Teklif raporları sayfası.
        /// </summary>
        public async Task<IActionResult> QuotationReport(QuotationReportFilter filter)
        {
            var companyId = GetCurrentCompanyId();
            
            filter.StartDate ??= DateTime.Now.AddMonths(-3);
            filter.EndDate ??= DateTime.Now;
            
            var query = _context.Quotations
                .Include(q => q.RFQ)
                .Where(q => q.SupplierCompanyId == companyId && q.IsActive);
            
            if (filter.StartDate.HasValue)
                query = query.Where(q => q.CreatedAt >= filter.StartDate.Value);
            
            if (filter.EndDate.HasValue)
                query = query.Where(q => q.CreatedAt <= filter.EndDate.Value.AddDays(1));
            
            if (filter.Status.HasValue)
                query = query.Where(q => q.Status == filter.Status.Value);
            
            var quotations = await query.ToListAsync();
            
            // Statistics
            var totalQuotations = quotations.Count;
            var submittedCount = quotations.Count(q => q.Status != QuotationStatus.Draft);
            var approvedCount = quotations.Count(q => q.Status == QuotationStatus.Approved || q.Status == QuotationStatus.PartiallyApproved);
            var rejectedCount = quotations.Count(q => q.Status == QuotationStatus.Rejected);
            
            var totalAmount = quotations.Sum(q => q.TotalAmount);
            var approvedAmount = quotations
                .Where(q => q.Status == QuotationStatus.Approved || q.Status == QuotationStatus.PartiallyApproved)
                .Sum(q => q.TotalAmount);
            
            var winRate = submittedCount > 0 ? (double)approvedCount / submittedCount * 100 : 0;
            
            // Monthly amounts
            var monthlyAmounts = quotations
                .GroupBy(q => new { q.CreatedAt.Year, q.CreatedAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyAmountPoint
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                    TotalAmount = g.Sum(x => x.TotalAmount),
                    ApprovedAmount = g.Where(x => x.Status == QuotationStatus.Approved || x.Status == QuotationStatus.PartiallyApproved).Sum(x => x.TotalAmount)
                })
                .ToList();
            
            // Status distribution for pie chart
            var statusDistribution = quotations
                .GroupBy(q => q.Status)
                .Select(g => new ChartDataPoint
                {
                    Label = GetQuotationStatusText(g.Key),
                    Value = g.Count()
                })
                .ToList();
            
            var viewModel = new QuotationReportViewModel
            {
                Filter = filter,
                TotalQuotations = totalQuotations,
                SubmittedCount = submittedCount,
                ApprovedCount = approvedCount,
                RejectedCount = rejectedCount,
                TotalAmount = totalAmount,
                ApprovedAmount = approvedAmount,
                WinRate = winRate,
                MonthlyAmounts = monthlyAmounts,
                StatusDistribution = statusDistribution
            };
            
            return View(viewModel);
        }

        private string GetQuotationStatusText(QuotationStatus status) => status switch
        {
            QuotationStatus.Draft => "Taslak",
            QuotationStatus.Submitted => "Gönderildi",
            QuotationStatus.Approved => "Onaylandı",
            QuotationStatus.Rejected => "Reddedildi",
            QuotationStatus.Withdrawn => "Geri Çekildi",
            QuotationStatus.PartiallyApproved => "Kısmi Onay",
            _ => "Diğer"
        };

        #endregion

        #region Supplier Performance

        /// <summary>
        /// Tedarikçi performans raporu.
        /// </summary>
        public async Task<IActionResult> SupplierPerformance()
        {
            var companyId = GetCurrentCompanyId();
            
            // Get all quotations for RFQs owned by this company
            var rfqIds = await _context.RFQs
                .Where(r => r.CompanyId == companyId && r.IsActive)
                .Select(r => r.Id)
                .ToListAsync();
            
            var quotations = await _context.Quotations
                .Include(q => q.SupplierCompany)
                .Where(q => rfqIds.Contains(q.RfqId) && q.IsActive && q.Status != QuotationStatus.Draft)
                .ToListAsync();
            
            // Group by supplier
            var supplierStats = quotations
                .GroupBy(q => new { q.SupplierCompanyId, q.SupplierCompany?.CompanyName })
                .Select(g => new SupplierPerformanceItem
                {
                    SupplierName = g.Key.CompanyName ?? "Bilinmiyor",
                    TotalQuotations = g.Count(),
                    ApprovedQuotations = g.Count(q => q.Status == QuotationStatus.Approved || q.Status == QuotationStatus.PartiallyApproved),
                    TotalAmount = g.Sum(q => q.TotalAmount),
                    AverageAmount = g.Average(q => q.TotalAmount),
                    WinRate = g.Count() > 0 
                        ? (double)g.Count(q => q.Status == QuotationStatus.Approved || q.Status == QuotationStatus.PartiallyApproved) / g.Count() * 100 
                        : 0
                })
                .OrderByDescending(s => s.ApprovedQuotations)
                .ToList();
            
            // Total stats
            var totalSuppliers = supplierStats.Count;
            var totalQuotations = quotations.Count;
            var avgQuotationsPerSupplier = totalSuppliers > 0 ? (double)totalQuotations / totalSuppliers : 0;
            
            // Top suppliers for chart
            var topSuppliers = supplierStats.Take(10).ToList();
            
            var viewModel = new SupplierPerformanceViewModel
            {
                TotalSuppliers = totalSuppliers,
                TotalQuotations = totalQuotations,
                AvgQuotationsPerSupplier = avgQuotationsPerSupplier,
                Suppliers = supplierStats,
                TopSuppliersChart = topSuppliers.Select(s => new ChartDataPoint
                {
                    Label = s.SupplierName,
                    Value = (int)s.ApprovedQuotations
                }).ToList()
            };
            
            return View(viewModel);
        }

        #endregion

        #region Material Report

        /// <summary>
        /// Malzeme raporu.
        /// </summary>
        public async Task<IActionResult> MaterialReport()
        {
            var companyId = GetCurrentCompanyId();
            
            // Get RFQ items with materials
            var rfqItems = await _context.RFQItems
                .Include(ri => ri.Material)
                .Include(ri => ri.RFQ)
                .Where(ri => ri.RFQ!.CompanyId == companyId && ri.IsActive && ri.MaterialId != null)
                .ToListAsync();
            
            // Most requested materials
            var topMaterials = rfqItems
                .Where(ri => ri.Material != null)
                .GroupBy(ri => new { ri.MaterialId, ri.Material!.Name, ri.Material.Unit })
                .Select(g => new MaterialReportItem
                {
                    MaterialName = g.Key.Name,
                    Unit = g.Key.Unit,
                    RequestCount = g.Count(),
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(m => m.RequestCount)
                .Take(10)
                .ToList();
            
            // Get quotation items for price trends
            var materialIds = topMaterials.Select(m => m.MaterialName).ToList();
            
            var quotationItems = await _context.QuotationItems
                .Include(qi => qi.RFQItem)
                .ThenInclude(ri => ri!.Material)
                .Where(qi => qi.IsActive && qi.RFQItem != null && qi.RFQItem.Material != null)
                .Where(qi => materialIds.Contains(qi.RFQItem!.Material!.Name))
                .Select(qi => new
                {
                    MaterialName = qi.RFQItem!.Material!.Name,
                    qi.UnitPrice,
                    qi.CreatedAt
                })
                .ToListAsync();
            
            // Price trend for top 3 materials
            var priceTrends = quotationItems
                .GroupBy(qi => qi.MaterialName)
                .Select(g => new MaterialPriceTrend
                {
                    MaterialName = g.Key,
                    PricePoints = g
                        .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
                        .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month)
                        .Select(x => new PricePoint
                        {
                            Label = $"{x.Key.Year}-{x.Key.Month:D2}",
                            AvgPrice = x.Average(p => p.UnitPrice),
                            MinPrice = x.Min(p => p.UnitPrice),
                            MaxPrice = x.Max(p => p.UnitPrice)
                        })
                        .ToList()
                })
                .Take(3)
                .ToList();
            
            var viewModel = new MaterialReportViewModel
            {
                TotalMaterials = topMaterials.Count,
                TotalRequests = rfqItems.Count,
                TopMaterials = topMaterials,
                PriceTrends = priceTrends,
                TopMaterialsChart = topMaterials.Select(m => new ChartDataPoint
                {
                    Label = m.MaterialName,
                    Value = m.RequestCount
                }).ToList()
            };
            
            return View(viewModel);
        }

        #endregion
    }

    #region ViewModels and Filters

    // ===== Filters =====
    public class RfqReportFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public RfqStatus? Status { get; set; }
        public int? SectorId { get; set; }
    }

    public class QuotationReportFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public QuotationStatus? Status { get; set; }
    }

    // ===== RFQ Report =====
    public class RfqReportViewModel
    {
        public RfqReportFilter Filter { get; set; } = new();
        public List<SelectListItem> Sectors { get; set; } = new();
        
        public int TotalRfqs { get; set; }
        public int DraftCount { get; set; }
        public int PublishedCount { get; set; }
        public int ClosedCount { get; set; }
        public int CancelledCount { get; set; }
        public int TotalQuotations { get; set; }
        public double AvgQuotationsPerRfq { get; set; }
        
        public List<MonthlyDataPoint> MonthlyData { get; set; } = new();
        public List<ChartDataPoint> SectorDistribution { get; set; } = new();
    }

    // ===== Quotation Report =====
    public class QuotationReportViewModel
    {
        public QuotationReportFilter Filter { get; set; } = new();
        
        public int TotalQuotations { get; set; }
        public int SubmittedCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public double WinRate { get; set; }
        
        public List<MonthlyAmountPoint> MonthlyAmounts { get; set; } = new();
        public List<ChartDataPoint> StatusDistribution { get; set; } = new();
    }

    // ===== Supplier Performance =====
    public class SupplierPerformanceViewModel
    {
        public int TotalSuppliers { get; set; }
        public int TotalQuotations { get; set; }
        public double AvgQuotationsPerSupplier { get; set; }
        
        public List<SupplierPerformanceItem> Suppliers { get; set; } = new();
        public List<ChartDataPoint> TopSuppliersChart { get; set; } = new();
    }

    public class SupplierPerformanceItem
    {
        public string SupplierName { get; set; } = string.Empty;
        public int TotalQuotations { get; set; }
        public int ApprovedQuotations { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public double WinRate { get; set; }
    }

    // ===== Material Report =====
    public class MaterialReportViewModel
    {
        public int TotalMaterials { get; set; }
        public int TotalRequests { get; set; }
        
        public List<MaterialReportItem> TopMaterials { get; set; } = new();
        public List<MaterialPriceTrend> PriceTrends { get; set; } = new();
        public List<ChartDataPoint> TopMaterialsChart { get; set; } = new();
    }

    public class MaterialReportItem
    {
        public string MaterialName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int RequestCount { get; set; }
        public decimal TotalQuantity { get; set; }
    }

    public class MaterialPriceTrend
    {
        public string MaterialName { get; set; } = string.Empty;
        public List<PricePoint> PricePoints { get; set; } = new();
    }

    public class PricePoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal AvgPrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
    }

    // ===== Chart Data =====
    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class MonthlyDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class MonthlyAmountPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
    }

    #endregion
}
