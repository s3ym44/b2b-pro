using B2BProcurement.Core.Enums;

namespace B2BProcurement.Core.Entities
{
    /// <summary>
    /// Bildirim entity'si.
    /// Kullanıcılara gönderilen bildirimleri tanımlar.
    /// </summary>
    public class Notification : BaseEntity
    {
        /// <summary>
        /// Kullanıcı ID (Foreign Key).
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Bildirim başlığı.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Bildirim mesajı.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Bildirim türü.
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// Okundu mu?
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Okunma tarihi.
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// İlgili entity kimliği (RFQ, Quotation vb.).
        /// </summary>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// İlgili entity tipi.
        /// </summary>
        public string? RelatedEntityType { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Bildirimin gönderildiği kullanıcı.
        /// </summary>
        public virtual User? User { get; set; }

        #endregion
    }
}
