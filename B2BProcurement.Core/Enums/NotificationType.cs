namespace B2BProcurement.Core.Enums
{
    /// <summary>
    /// Bildirim türleri.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Bilgilendirme.
        /// </summary>
        Info = 0,

        /// <summary>
        /// Yeni RFQ yayınlandı.
        /// </summary>
        NewRfq = 1,

        /// <summary>
        /// Yeni teklif alındı.
        /// </summary>
        NewQuotation = 2,

        /// <summary>
        /// Teklif onaylandı.
        /// </summary>
        QuotationApproved = 3,

        /// <summary>
        /// Teklif reddedildi.
        /// </summary>
        QuotationRejected = 4,

        /// <summary>
        /// RFQ süresi dolmak üzere.
        /// </summary>
        RfqExpiring = 5,

        /// <summary>
        /// Uyarı.
        /// </summary>
        Warning = 6,

        /// <summary>
        /// Sistem bildirimi.
        /// </summary>
        System = 7,

        /// <summary>
        /// Başarılı işlem.
        /// </summary>
        Success = 8,

        /// <summary>
        /// Hata.
        /// </summary>
        Error = 9
    }
}
