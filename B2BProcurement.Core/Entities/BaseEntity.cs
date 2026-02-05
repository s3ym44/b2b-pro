namespace B2BProcurement.Core.Entities
{
    using B2BProcurement.Core.Interfaces;

    /// <summary>
    /// Tüm entity'ler için temel sınıf.
    /// </summary>
    public abstract class BaseEntity : IEntity, IAuditableEntity, ISoftDelete
    {
        /// <summary>
        /// Entity'nin benzersiz kimliği.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Oluşturulma tarihi.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Son güncelleme tarihi.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Aktiflik durumu. False ise soft deleted.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
