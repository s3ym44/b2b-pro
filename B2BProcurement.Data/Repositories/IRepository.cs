using System.Linq.Expressions;
using B2BProcurement.Core.Interfaces;

namespace B2BProcurement.Data.Repositories
{
    /// <summary>
    /// Generic Repository arayüzü.
    /// Tüm CRUD işlemleri için temel metotları tanımlar.
    /// </summary>
    /// <typeparam name="T">Entity tipi (IEntity ve ISoftDelete implement etmeli).</typeparam>
    public interface IRepository<T> where T : class, IEntity, ISoftDelete
    {
        /// <summary>
        /// Id'ye göre kayıt getirir.
        /// </summary>
        /// <param name="id">Kayıt kimliği.</param>
        /// <returns>Bulunan entity veya null.</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Tüm kayıtları getirir.
        /// </summary>
        /// <returns>Entity listesi.</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Belirtilen koşula göre kayıtları getirir.
        /// </summary>
        /// <param name="predicate">Filtreleme koşulu.</param>
        /// <returns>Koşula uyan entity listesi.</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Yeni kayıt ekler.
        /// </summary>
        /// <param name="entity">Eklenecek entity.</param>
        /// <returns>Eklenen entity.</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Mevcut kaydı günceller.
        /// </summary>
        /// <param name="entity">Güncellenecek entity.</param>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Kaydı soft delete yapar (IsActive = false).
        /// </summary>
        /// <param name="entity">Silinecek entity.</param>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Belirtilen Id'ye sahip kayıt var mı kontrol eder.
        /// </summary>
        /// <param name="id">Kontrol edilecek Id.</param>
        /// <returns>Kayıt varsa true, yoksa false.</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Sorgulanabilir IQueryable döndürür.
        /// LINQ sorguları için kullanılır.
        /// </summary>
        /// <returns>IQueryable nesnesi.</returns>
        IQueryable<T> Query();

        /// <summary>
        /// Değişiklikleri veritabanına kaydeder.
        /// </summary>
        /// <returns>Etkilenen kayıt sayısı.</returns>
        Task<int> SaveChangesAsync();
    }
}
