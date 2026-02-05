namespace B2BProcurement.Core.Interfaces
{
    /// <summary>
    /// Soft delete özelliği olan entity'ler için arayüz.
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// Aktiflik durumu. False ise soft deleted.
        /// </summary>
        bool IsActive { get; set; }
    }
}
