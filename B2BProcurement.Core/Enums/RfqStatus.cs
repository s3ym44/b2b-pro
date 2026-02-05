namespace B2BProcurement.Core.Enums
{
    /// <summary>
    /// RFQ (Teklif Talebi) durum türleri.
    /// </summary>
    public enum RfqStatus
    {
        /// <summary>
        /// Taslak - Henüz yayınlanmamış.
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Yayınlandı - Tedarikçilere açık.
        /// </summary>
        Published = 1,

        /// <summary>
        /// İnceleme altında - Teklifler değerlendiriliyor.
        /// </summary>
        UnderReview = 2,

        /// <summary>
        /// Kapatıldı - Teklif alma süresi doldu.
        /// </summary>
        Closed = 3,

        /// <summary>
        /// Tamamlandı - İşlem başarıyla tamamlandı.
        /// </summary>
        Completed = 4,

        /// <summary>
        /// İptal edildi.
        /// </summary>
        Cancelled = 5
    }
}
