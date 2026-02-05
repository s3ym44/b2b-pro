namespace B2BProcurement.Core.Entities
{
    /// <summary>
    /// RFQ iletişim kişisi entity'si.
    /// RFQ için yetkili iletişim kişilerini tanımlar.
    /// </summary>
    public class RFQContact : BaseEntity
    {
        /// <summary>
        /// RFQ ID (Foreign Key).
        /// </summary>
        public int RfqId { get; set; }

        /// <summary>
        /// İletişim kişisi adı.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// E-posta adresi.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Telefon numarası.
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Birincil iletişim kişisi mi?
        /// </summary>
        public bool IsPrimary { get; set; }

        #region Navigation Properties

        /// <summary>
        /// İletişim kişisinin ait olduğu RFQ.
        /// </summary>
        public virtual RFQ? RFQ { get; set; }

        #endregion
    }
}
