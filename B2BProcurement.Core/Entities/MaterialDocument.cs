using B2BProcurement.Core.Enums;

namespace B2BProcurement.Core.Entities
{
    /// <summary>
    /// Malzeme dokümanı entity'si.
    /// Malzemelere ait teknik şartname, görsel vb. dosyaları tanımlar.
    /// </summary>
    public class MaterialDocument : BaseEntity
    {
        /// <summary>
        /// Malzeme ID (Foreign Key).
        /// </summary>
        public int MaterialId { get; set; }

        /// <summary>
        /// Dosya adı.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Dosya yolu.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Dosya tipi (pdf, jpg, vb.).
        /// </summary>
        public string? FileType { get; set; }

        /// <summary>
        /// Dosya boyutu (byte).
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Doküman türü.
        /// </summary>
        public DocumentType DocumentType { get; set; }

        #region Navigation Properties

        /// <summary>
        /// Dokümanın ait olduğu malzeme.
        /// </summary>
        public virtual Material? Material { get; set; }

        #endregion
    }
}
