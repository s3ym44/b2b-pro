namespace B2BProcurement.Core.DTOs
{
    /// <summary>
    /// Tüm DTO'lar için temel sınıf.
    /// </summary>
    public abstract class BaseDto
    {
        /// <summary>
        /// DTO'nun karşılık geldiği entity'nin kimliği.
        /// </summary>
        public int Id { get; set; }
    }
}
