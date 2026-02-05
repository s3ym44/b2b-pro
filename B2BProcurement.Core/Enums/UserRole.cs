namespace B2BProcurement.Core.Enums
{
    /// <summary>
    /// Kullanıcı rol türleri.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Sistem yöneticisi - Tüm yetkilere sahip.
        /// </summary>
        SystemAdmin = 0,

        /// <summary>
        /// Şirket yöneticisi - Şirket genelinde yetkilere sahip.
        /// </summary>
        CompanyAdmin = 1,

        /// <summary>
        /// Satın alma yöneticisi - Satın alma işlemlerini yönetir.
        /// </summary>
        PurchaseManager = 2,

        /// <summary>
        /// Standart kullanıcı.
        /// </summary>
        User = 3
    }
}
