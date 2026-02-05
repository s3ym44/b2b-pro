namespace B2BProcurement.Core.Entities
{
    /// <summary>
    /// Teklif dokümanı entity'si.
    /// Tekliflere eklenen dosyaları tanımlar.
    /// </summary>
    public class QuotationDocument : BaseEntity
    {
        /// <summary>
        /// Teklif ID (Foreign Key).
        /// </summary>
        public int QuotationId { get; set; }

        /// <summary>
        /// Dosya adı.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Dosya yolu.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        #region Navigation Properties

        /// <summary>
        /// Dokümanın ait olduğu teklif.
        /// </summary>
        public virtual Quotation? Quotation { get; set; }

        #endregion
    }
}
