namespace B2BProcurement.Core.Interfaces
{
    /// <summary>
    /// Tüm entity'ler için temel arayüz.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Entity'nin benzersiz kimliği.
        /// </summary>
        int Id { get; set; }
    }
}
