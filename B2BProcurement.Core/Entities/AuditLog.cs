namespace B2BProcurement.Core.Entities
{
    /// <summary>
    /// Denetim kaydı entity'si.
    /// Sistemdeki tüm değişikliklerin loglarını tutar.
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// Log ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// İşlemi yapan kullanıcı ID.
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Yapılan işlem (Create, Update, Delete vb.).
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Etkilenen entity tipi (Company, User vb.).
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// Etkilenen entity ID.
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Eski değerler (JSON formatında).
        /// </summary>
        public string? OldValues { get; set; }

        /// <summary>
        /// Yeni değerler (JSON formatında).
        /// </summary>
        public string? NewValues { get; set; }

        /// <summary>
        /// İşlemi yapan IP adresi.
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Log oluşturulma tarihi.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
