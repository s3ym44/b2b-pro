using B2BProcurement.Core.Enums;

namespace B2BProcurement.Core.Entities
{
    /// <summary>
    /// RFQ dokümanı entity'si.
    /// RFQ'ya eklenen dosyaları tanımlar.
    /// </summary>
    public class RFQDocument : BaseEntity
    {
        /// <summary>
        /// RFQ ID (Foreign Key).
        /// </summary>
        public int RfqId { get; set; }

        /// <summary>
        /// Dosya adı.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Dosya yolu.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Doküman türü.
        /// </summary>
        public DocumentType DocumentType { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Dokümanın ait olduğu RFQ.
        /// </summary>
        public virtual RFQ? RFQ { get; set; }

        #endregion
    }
}
