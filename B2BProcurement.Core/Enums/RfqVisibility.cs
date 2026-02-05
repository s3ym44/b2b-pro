namespace B2BProcurement.Core.Enums
{
    /// <summary>
    /// RFQ görünürlük türleri.
    /// </summary>
    public enum RfqVisibility
    {
        /// <summary>
        /// Sadece şirketin tedarikçilerine görünür.
        /// </summary>
        MySuppliers = 0,

        /// <summary>
        /// Sektördeki tüm tedarikçilere görünür.
        /// </summary>
        AllSector = 1,

        /// <summary>
        /// Seçilen tedarikçilere görünür.
        /// </summary>
        Selected = 2
    }
}
