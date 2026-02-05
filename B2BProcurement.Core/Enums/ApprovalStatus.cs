namespace B2BProcurement.Core.Enums
{
    /// <summary>
    /// Onay durum türleri.
    /// </summary>
    public enum ApprovalStatus
    {
        /// <summary>
        /// Beklemede - Henüz onaylanmamış.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Onaylandı.
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Reddedildi.
        /// </summary>
        Rejected = 2,

        /// <summary>
        /// Kısmi onaylandı - Miktar kısmen onaylandı.
        /// </summary>
        PartialApproved = 3
    }
}
