namespace B2BProcurement.Core.Interfaces
{
    /// <summary>
    /// Denetlenebilir entity'ler için arayüz.
    /// CreatedAt ve UpdatedAt alanlarını otomatik yönetmek için kullanılır.
    /// </summary>
    public interface IAuditableEntity
    {
        /// <summary>
        /// Oluşturulma tarihi.
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Son güncelleme tarihi.
        /// </summary>
        DateTime? UpdatedAt { get; set; }
    }
}
