namespace B2BProcurement.Business.Interfaces
{
    /// <summary>
    /// Generic Service arayüzü.
    /// İş mantığı katmanı için temel metotları tanımlar.
    /// </summary>
    /// <typeparam name="TDto">DTO tipi.</typeparam>
    public interface IService<TDto> where TDto : class
    {
        /// <summary>
        /// Tüm kayıtları getirir.
        /// </summary>
        /// <returns>DTO listesi.</returns>
        Task<IEnumerable<TDto>> GetAllAsync();

        /// <summary>
        /// Id'ye göre kayıt getirir.
        /// </summary>
        /// <param name="id">Kayıt kimliği.</param>
        /// <returns>Bulunan DTO veya null.</returns>
        Task<TDto?> GetByIdAsync(int id);

        /// <summary>
        /// Yeni kayıt ekler.
        /// </summary>
        /// <param name="dto">Eklenecek DTO.</param>
        /// <returns>Eklenen DTO.</returns>
        Task<TDto> AddAsync(TDto dto);

        /// <summary>
        /// Mevcut kaydı günceller.
        /// </summary>
        /// <param name="dto">Güncellenecek DTO.</param>
        /// <returns>Başarılı ise true.</returns>
        Task<bool> UpdateAsync(TDto dto);

        /// <summary>
        /// Kaydı siler.
        /// </summary>
        /// <param name="id">Silinecek kayıt kimliği.</param>
        /// <returns>Başarılı ise true.</returns>
        Task<bool> DeleteAsync(int id);
    }
}
