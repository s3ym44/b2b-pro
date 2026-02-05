namespace B2BProcurement.Core.Enums
{
    /// <summary>
    /// Teklif durum türleri.
    /// </summary>
    public enum QuotationStatus
    {
        /// <summary>
        /// Taslak - Henüz gönderilmemiş.
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Gönderildi - Değerlendirme bekliyor.
        /// </summary>
        Submitted = 1,

        /// <summary>
        /// Onaylandı - Teklif kabul edildi.
        /// </summary>
        Approved = 2,

        /// <summary>
        /// Reddedildi - Teklif kabul edilmedi.
        /// </summary>
        Rejected = 3,

        /// <summary>
        /// Geri çekildi - Tedarikçi tarafından geri alındı.
        /// </summary>
        Withdrawn = 4,

        /// <summary>
        /// Tamamlandı - Sipariş oluşturuldu.
        /// </summary>
        Completed = 5,

        /// <summary>
        /// Kısmi Onaylandı - Bazı kalemler kabul edildi.
        /// </summary>
        PartiallyApproved = 6
    }
}
