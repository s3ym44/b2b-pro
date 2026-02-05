using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using B2BProcurement.Business.DTOs.Notification;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Business.Hubs;
using B2BProcurement.Core.Entities;
using B2BProcurement.Core.Enums;
using B2BProcurement.Data.Context;

namespace B2BProcurement.Business.Services
{
    /// <summary>
    /// Bildirim servisi implementasyonu.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub>? _hubContext;

        public NotificationService(
            ApplicationDbContext context,
            IMapper mapper,
            IHubContext<NotificationHub>? hubContext = null)
        {
            _context = context;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        #region Basic Operations

        public async Task<NotificationDto?> GetByIdAsync(int id)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.IsActive);
            return _mapper.Map<NotificationDto>(notification);
        }

        public async Task<IEnumerable<NotificationDto>> GetByUserAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.IsActive)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }

        public async Task<IEnumerable<NotificationDto>> GetUnreadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && n.IsActive)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10) // Son 10 okunmamış
                .ToListAsync();
            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead && n.IsActive);
        }

        public async Task CreateAsync(int userId, string title, string message, NotificationType type,
            int? relatedEntityId = null, string? relatedEntityType = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType,
                IsRead = false,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification via SignalR
            await SendRealTimeNotificationAsync(userId, notification);
        }

        public async Task CreateForCompanyAsync(int companyId, string title, string message, NotificationType type)
        {
            var userIds = await _context.Users
                .Where(u => u.CompanyId == companyId && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var userId in userIds)
            {
                await CreateAsync(userId, title, message, type);
            }
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                notification.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && n.IsActive)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                notification.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsActive = false;
                notification.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> CleanupOldNotificationsAsync(int olderThanDays = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-olderThanDays);
            var oldNotifications = await _context.Notifications
                .Where(n => n.CreatedAt < cutoffDate && n.IsRead)
                .ToListAsync();

            foreach (var notification in oldNotifications)
            {
                notification.IsActive = false;
            }

            await _context.SaveChangesAsync();
            return oldNotifications.Count;
        }

        #endregion

        #region Pre-defined Notifications

        public async Task NotifyNewRfqAsync(int rfqId)
        {
            var rfq = await _context.RFQs
                .Include(r => r.Sector)
                .FirstOrDefaultAsync(r => r.Id == rfqId);

            if (rfq == null) return;

            // Find all companies in the same sector (potential suppliers)
            var companyIds = await _context.Companies
                .Where(c => c.SectorId == rfq.SectorId && c.Id != rfq.CompanyId && c.IsActive)
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var companyId in companyIds)
            {
                await CreateForCompanyAsync(
                    companyId,
                    "🆕 Yeni Teklif Talebi",
                    $"'{rfq.Title}' başlıklı yeni bir RFQ yayınlandı. Son teklif tarihi: {rfq.EndDate:dd.MM.yyyy}",
                    NotificationType.NewRfq);
            }
        }

        public async Task NotifyNewQuotationAsync(int quotationId)
        {
            var quotation = await _context.Quotations
                .Include(q => q.RFQ)
                .Include(q => q.SupplierCompany)
                .FirstOrDefaultAsync(q => q.Id == quotationId);

            if (quotation?.RFQ == null) return;

            // Notify RFQ owner company
            await CreateForCompanyAsync(
                quotation.RFQ.CompanyId,
                "📥 Yeni Teklif Alındı",
                $"'{quotation.RFQ.Title}' için {quotation.SupplierCompany?.CompanyName ?? "Tedarikçi"} firmasından yeni teklif alındı.",
                NotificationType.NewQuotation);
        }

        public async Task NotifyQuotationApprovedAsync(int quotationId)
        {
            var quotation = await _context.Quotations
                .Include(q => q.RFQ)
                .FirstOrDefaultAsync(q => q.Id == quotationId);

            if (quotation == null) return;

            // Notify supplier company
            await CreateForCompanyAsync(
                quotation.SupplierCompanyId,
                "✅ Teklifiniz Onaylandı",
                $"'{quotation.RFQ?.Title}' için verdiğiniz teklif onaylandı!",
                NotificationType.QuotationApproved);
        }

        public async Task NotifyQuotationRejectedAsync(int quotationId)
        {
            var quotation = await _context.Quotations
                .Include(q => q.RFQ)
                .FirstOrDefaultAsync(q => q.Id == quotationId);

            if (quotation == null) return;

            // Notify supplier company
            await CreateForCompanyAsync(
                quotation.SupplierCompanyId,
                "❌ Teklifiniz Reddedildi",
                $"'{quotation.RFQ?.Title}' için verdiğiniz teklif reddedildi.",
                NotificationType.QuotationRejected);
        }

        public async Task NotifyRfqExpiringAsync(int rfqId, int daysRemaining)
        {
            var rfq = await _context.RFQs
                .Include(r => r.Company)
                .FirstOrDefaultAsync(r => r.Id == rfqId);

            if (rfq == null) return;

            // Notify RFQ owner
            await CreateForCompanyAsync(
                rfq.CompanyId,
                $"⏰ RFQ Süresi Dolmak Üzere",
                $"'{rfq.Title}' başlıklı RFQ'nun süresi {daysRemaining} gün içinde dolacak.",
                NotificationType.RfqExpiring);

            // Also notify suppliers who have draft quotations
            var supplierCompanyIds = await _context.Quotations
                .Where(q => q.RfqId == rfqId && q.Status == QuotationStatus.Draft && q.IsActive)
                .Select(q => q.SupplierCompanyId)
                .Distinct()
                .ToListAsync();

            foreach (var companyId in supplierCompanyIds)
            {
                await CreateForCompanyAsync(
                    companyId,
                    $"⏰ Taslak Teklifinizi Gönderin",
                    $"'{rfq.Title}' için taslak teklifiniz var. RFQ süresi {daysRemaining} gün içinde dolacak!",
                    NotificationType.RfqExpiring);
            }
        }

        #endregion

        #region SignalR Real-time

        private async Task SendRealTimeNotificationAsync(int userId, Notification notification)
        {
            if (_hubContext == null) return;

            try
            {
                await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type.ToString().ToLower(),
                    createdAt = notification.CreatedAt
                });
            }
            catch
            {
                // SignalR error, ignore
            }
        }

        #endregion
    }
}
