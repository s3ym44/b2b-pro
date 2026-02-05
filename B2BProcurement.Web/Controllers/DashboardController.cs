using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using B2BProcurement.Core.Enums;
using B2BProcurement.Data.Context;
using System.Security.Claims;

namespace B2BProcurement.Controllers
{
    /// <summary>
    /// Dashboard controller - Ana sayfa ve özet bilgiler.
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Dashboard ana sayfası.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var companyId = GetCurrentCompanyId();
            var userId = GetCurrentUserId();
            var now = DateTime.UtcNow;
            var sixMonthsAgo = now.AddMonths(-6);

            // Özet Kartları - SQLite decimal Sum desteklemediği için client-side hesapla
            var monthlyQuotations = await _context.Quotations
                .Where(q => q.SupplierCompanyId == companyId && 
                    q.Status == QuotationStatus.Approved &&
                    q.CreatedAt.Month == now.Month &&
                    q.CreatedAt.Year == now.Year)
                .Select(q => q.TotalAmount)
                .ToListAsync();

            var summary = new DashboardSummaryDto
            {
                ActiveQuotations = await _context.Quotations
                    .CountAsync(q => q.SupplierCompanyId == companyId && 
                        (q.Status == QuotationStatus.Draft || q.Status == QuotationStatus.Submitted)),
                
                IncomingRfqs = await _context.RFQs
                    .CountAsync(r => r.Status == RfqStatus.Published && 
                        r.EndDate > now),
                
                PendingResponses = await _context.Quotations
                    .CountAsync(q => q.RFQ != null && q.RFQ.CompanyId == companyId && 
                        q.Status == QuotationStatus.Submitted),
                
                MonthlyTotal = monthlyQuotations.Sum()
            };

            // Son 6 Ay Teklif Trendi
            var trendData = await GetMonthlyTrendAsync(companyId, sixMonthsAgo);

            // Son Aktiviteler
            var recentActivities = await GetRecentActivitiesAsync(companyId, userId);

            // Gelen RFQ'lar
            var incomingRfqs = await _context.RFQs
                .Include(r => r.Company)
                .Include(r => r.Items)
                .Where(r => r.Status == RfqStatus.Published && r.EndDate > now)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new IncomingRfqDto
                {
                    Id = r.Id,
                    RfqNumber = r.RfqNumber,
                    Title = r.Title,
                    CompanyName = r.Company != null ? r.Company.CompanyName : "Bilinmiyor",
                    Sector = r.Company != null && r.Company.Sector != null ? r.Company.Sector.Name : "Belirtilmemiş",
                    Deadline = r.EndDate,
                    ItemCount = r.Items.Count,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            // Yaklaşan Bitiş Tarihleri (7 gün içinde)
            var upcomingDeadlines = await _context.RFQs
                .Include(r => r.Company)
                .Where(r => (r.CompanyId == companyId || r.Status == RfqStatus.Published) &&
                    r.EndDate > now && 
                    r.EndDate <= now.AddDays(7))
                .OrderBy(r => r.EndDate)
                .Take(5)
                .Select(r => new UpcomingDeadlineDto
                {
                    Id = r.Id,
                    RfqNumber = r.RfqNumber,
                    Title = r.Title,
                    Deadline = r.EndDate,
                    DaysRemaining = (int)(r.EndDate - now).TotalDays,
                    Status = r.Status
                })
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                Summary = summary,
                MonthlyTrend = trendData,
                RecentActivities = recentActivities,
                IncomingRfqs = incomingRfqs,
                UpcomingDeadlines = upcomingDeadlines
            };

            return View(viewModel);
        }

        private async Task<List<MonthlyTrendDto>> GetMonthlyTrendAsync(int companyId, DateTime startDate)
        {
            var result = new List<MonthlyTrendDto>();
            var now = DateTime.UtcNow;

            for (int i = 5; i >= 0; i--)
            {
                var month = now.AddMonths(-i);
                var startOfMonth = new DateTime(month.Year, month.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1);

                var rfqCount = await _context.RFQs
                    .CountAsync(r => r.CompanyId == companyId &&
                        r.CreatedAt >= startOfMonth && r.CreatedAt < endOfMonth);

                var quotationCount = await _context.Quotations
                    .CountAsync(q => q.SupplierCompanyId == companyId &&
                        q.CreatedAt >= startOfMonth && q.CreatedAt < endOfMonth);

                result.Add(new MonthlyTrendDto
                {
                    Month = month.ToString("MMM yyyy"),
                    RfqCount = rfqCount,
                    QuotationCount = quotationCount
                });
            }

            return result;
        }

        private async Task<List<RecentActivityDto>> GetRecentActivitiesAsync(int companyId, int userId)
        {
            var activities = new List<RecentActivityDto>();

            // Son RFQ'lar
            var recentRfqs = await _context.RFQs
                .Where(r => r.CompanyId == companyId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new RecentActivityDto
                {
                    Date = r.CreatedAt,
                    Type = "RFQ",
                    TypeIcon = "fa-file-alt",
                    TypeColor = "primary",
                    Description = $"#{r.RfqNumber} - {r.Title}",
                    Status = r.Status.ToString()
                })
                .ToListAsync();

            // Son Teklifler
            var recentQuotations = await _context.Quotations
                .Include(q => q.RFQ)
                .Where(q => q.SupplierCompanyId == companyId)
                .OrderByDescending(q => q.CreatedAt)
                .Take(5)
                .Select(q => new RecentActivityDto
                {
                    Date = q.CreatedAt,
                    Type = "Teklif",
                    TypeIcon = "fa-hand-holding-usd",
                    TypeColor = "success",
                    Description = q.RFQ != null ? $"#{q.RFQ.RfqNumber} için teklif - {q.TotalAmount:C0}" : $"Teklif - {q.TotalAmount:C0}",
                    Status = q.Status.ToString()
                })
                .ToListAsync();

            // Birleştir ve sırala
            activities.AddRange(recentRfqs);
            activities.AddRange(recentQuotations);

            return activities
                .OrderByDescending(a => a.Date)
                .Take(10)
                .ToList();
        }

        private int GetCurrentCompanyId()
        {
            var claim = User.FindFirst("CompanyId");
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }

    #region ViewModels

    public class DashboardViewModel
    {
        public DashboardSummaryDto Summary { get; set; } = new();
        public List<MonthlyTrendDto> MonthlyTrend { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
        public List<IncomingRfqDto> IncomingRfqs { get; set; } = new();
        public List<UpcomingDeadlineDto> UpcomingDeadlines { get; set; } = new();
    }

    public class DashboardSummaryDto
    {
        public int ActiveQuotations { get; set; }
        public int IncomingRfqs { get; set; }
        public int PendingResponses { get; set; }
        public decimal MonthlyTotal { get; set; }
    }

    public class MonthlyTrendDto
    {
        public string Month { get; set; } = string.Empty;
        public int RfqCount { get; set; }
        public int QuotationCount { get; set; }
    }

    public class RecentActivityDto
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string TypeIcon { get; set; } = string.Empty;
        public string TypeColor { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class IncomingRfqDto
    {
        public int Id { get; set; }
        public string RfqNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpcomingDeadlineDto
    {
        public int Id { get; set; }
        public string RfqNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public int DaysRemaining { get; set; }
        public RfqStatus Status { get; set; }
    }

    #endregion
}
