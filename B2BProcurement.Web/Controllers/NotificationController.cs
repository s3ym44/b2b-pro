using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Business.DTOs.Notification;
using B2BProcurement.Business.Hubs;
using System.Security.Claims;

namespace B2BProcurement.Controllers
{
    /// <summary>
    /// Bildirim controller'ı.
    /// AJAX endpoint'leri ile bildirim yönetimi.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationController(
            INotificationService notificationService,
            IHubContext<NotificationHub> hubContext)
        {
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        #region Helper Methods

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        #endregion

        /// <summary>
        /// Okunmamış bildirimleri getirir.
        /// </summary>
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var notifications = await _notificationService.GetUnreadAsync(userId);
            var count = await _notificationService.GetUnreadCountAsync(userId);

            return Ok(new
            {
                success = true,
                count = count,
                notifications = notifications.Select(n => new
                {
                    id = n.Id,
                    title = n.Title,
                    message = n.Message,
                    type = n.Type.ToString().ToLower(),
                    typeIcon = GetTypeIcon(n.Type),
                    createdAt = n.CreatedAt,
                    timeAgo = GetTimeAgo(n.CreatedAt),
                    url = GetNotificationUrl(n)
                })
            });
        }

        /// <summary>
        /// Tüm bildirimleri getirir (sayfalı).
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var notifications = await _notificationService.GetByUserAsync(userId);
            var total = notifications.Count();
            var paged = notifications.Skip((page - 1) * pageSize).Take(pageSize);

            return Ok(new
            {
                success = true,
                page = page,
                pageSize = pageSize,
                totalCount = total,
                totalPages = (int)Math.Ceiling((double)total / pageSize),
                notifications = paged.Select(n => new
                {
                    id = n.Id,
                    title = n.Title,
                    message = n.Message,
                    type = n.Type.ToString().ToLower(),
                    typeIcon = GetTypeIcon(n.Type),
                    isRead = n.IsRead,
                    createdAt = n.CreatedAt,
                    timeAgo = GetTimeAgo(n.CreatedAt),
                    url = GetNotificationUrl(n)
                })
            });
        }

        /// <summary>
        /// Okunmamış bildirim sayısını getirir.
        /// </summary>
        [HttpGet("count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { success = true, count = count });
        }

        /// <summary>
        /// Tek bildirimi okundu olarak işaretler.
        /// </summary>
        [HttpPost("read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var notification = await _notificationService.GetByIdAsync(id);
            if (notification == null || notification.UserId != userId)
                return NotFound(new { success = false, message = "Bildirim bulunamadı." });

            await _notificationService.MarkAsReadAsync(id);
            var newCount = await _notificationService.GetUnreadCountAsync(userId);

            return Ok(new { success = true, newCount = newCount });
        }

        /// <summary>
        /// Tüm bildirimleri okundu olarak işaretler.
        /// </summary>
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { success = true, newCount = 0 });
        }

        /// <summary>
        /// Bildirimi siler.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var notification = await _notificationService.GetByIdAsync(id);
            if (notification == null || notification.UserId != userId)
                return NotFound(new { success = false, message = "Bildirim bulunamadı." });

            await _notificationService.DeleteAsync(id);
            return Ok(new { success = true });
        }

        #region Helper Methods for Response

        private string GetTypeIcon(B2BProcurement.Core.Enums.NotificationType type)
        {
            return type switch
            {
                Core.Enums.NotificationType.Info => "fa-info-circle",
                Core.Enums.NotificationType.Success => "fa-check-circle",
                Core.Enums.NotificationType.Warning => "fa-exclamation-triangle",
                Core.Enums.NotificationType.Error => "fa-times-circle",
                Core.Enums.NotificationType.NewRfq => "fa-file-alt",
                Core.Enums.NotificationType.NewQuotation => "fa-file-invoice-dollar",
                Core.Enums.NotificationType.QuotationApproved => "fa-thumbs-up",
                Core.Enums.NotificationType.QuotationRejected => "fa-thumbs-down",
                Core.Enums.NotificationType.RfqExpiring => "fa-clock",
                _ => "fa-bell"
            };
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.Now - dateTime;

            if (span.TotalMinutes < 1) return "Az önce";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} dk önce";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} saat önce";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays} gün önce";
            if (span.TotalDays < 30) return $"{(int)(span.TotalDays / 7)} hafta önce";
            return dateTime.ToString("dd.MM.yyyy");
        }

        private string? GetNotificationUrl(NotificationDto notification)
        {
            if (notification.RelatedEntityId == null || string.IsNullOrEmpty(notification.RelatedEntityType))
                return null;

            return notification.RelatedEntityType switch
            {
                "RFQ" => $"/Rfq/Details/{notification.RelatedEntityId}",
                "Quotation" => $"/Quotation/Details/{notification.RelatedEntityId}",
                _ => null
            };
        }

        #endregion
    }
}
