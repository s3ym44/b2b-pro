using B2BProcurement.Core.Entities;
using B2BProcurement.Core.Enums;
using B2BProcurement.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace B2BProcurement.Data.Repositories
{
    /// <summary>
    /// RFQ için özelleştirilmiş repository implementasyonu.
    /// </summary>
    public class RfqRepository : Repository<RFQ>, IRfqRepository
    {
        /// <summary>
        /// RfqRepository yapıcı metodu.
        /// </summary>
        /// <param name="context">Veritabanı bağlamı.</param>
        public RfqRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<RFQ?> GetWithItemsAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Items)
                    .ThenInclude(i => i.Material)
                .Include(r => r.Documents)
                .Include(r => r.Contacts)
                .Include(r => r.Company)
                .Include(r => r.Sector)
                .Include(r => r.Quotations)
                    .ThenInclude(q => q.SupplierCompany)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RFQ>> GetByCompanyAsync(int companyId)
        {
            return await _dbSet
                .Include(r => r.Sector)
                .Include(r => r.Items)
                .Where(r => r.CompanyId == companyId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RFQ>> GetBySectorAsync(int sectorId)
        {
            return await _dbSet
                .Include(r => r.Company)
                .Include(r => r.Items)
                .Where(r => r.SectorId == sectorId && r.Status == RfqStatus.Published)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RFQ>> GetByStatusAsync(RfqStatus status)
        {
            return await _dbSet
                .Include(r => r.Company)
                .Include(r => r.Sector)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RFQ>> GetExpiredAsync()
        {
            var now = DateTime.Now;
            return await _dbSet
                .Include(r => r.Company)
                .Where(r => r.EndDate < now && r.Status == RfqStatus.Published)
                .OrderByDescending(r => r.EndDate)
                .ToListAsync();
        }
    }
}
