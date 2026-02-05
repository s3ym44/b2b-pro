namespace B2BProcurement.Core.Enums
{
    /// <summary>
    /// Genel durum değerleri için enum.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// Beklemede durumu.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Onaylandı durumu.
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Reddedildi durumu.
        /// </summary>
        Rejected = 2,

        /// <summary>
        /// İptal edildi durumu.
        /// </summary>
        Cancelled = 3
    }
}
