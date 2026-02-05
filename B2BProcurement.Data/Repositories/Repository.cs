using System.Linq.Expressions;
using B2BProcurement.Core.Interfaces;
using B2BProcurement.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace B2BProcurement.Data.Repositories
{
    /// <summary>
    /// Generic Repository implementasyonu.
    /// Tüm CRUD işlemlerini gerçekleştirir.
    /// Soft delete desteği vardır.
    /// </summary>
    /// <typeparam name="T">Entity tipi.</typeparam>
    public class Repository<T> : IRepository<T> where T : class, IEntity, ISoftDelete
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Repository yapıcı metodu.
        /// </summary>
        /// <param name="context">Veritabanı bağlamı.</param>
        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <inheritdoc/>
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        /// <inheritdoc/>
        public Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Soft delete uygular. Entity fiziksel olarak silinmez,
        /// IsActive = false yapılarak işaretlenir.
        /// </remarks>
        public Task DeleteAsync(T entity)
        {
            // Soft delete: IsActive = false
            entity.IsActive = false;
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet.AnyAsync(e => e.Id == id);
        }

        /// <inheritdoc/>
        public IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
