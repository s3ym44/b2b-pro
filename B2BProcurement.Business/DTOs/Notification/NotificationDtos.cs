using B2BProcurement.Core.Enums;

namespace B2BProcurement.Business.DTOs.Notification
{
    /// <summary>
    /// Bildirim DTO'su.
    /// </summary>
    public class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
    }

    /// <summary>
    /// Bildirim oluşturma DTO'su.
    /// </summary>
    public class NotificationCreateDto
    {
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
    }
}
