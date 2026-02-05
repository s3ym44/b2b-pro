using B2BProcurement.Core.Entities;
using B2BProcurement.Core.Enums;
using B2BProcurement.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace B2BProcurement.Data.Repositories
{
    /// <summary>
    /// Quotation için özelleştirilmiş repository implementasyonu.
    /// </summary>
    public class QuotationRepository : Repository<Quotation>, IQuotationRepository
    {
        /// <summary>
        /// QuotationRepository yapıcı metodu.
        /// </summary>
        /// <param name="context">Veritabanı bağlamı.</param>
        public QuotationRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<Quotation?> GetWithItemsAsync(int id)
        {
            return await _dbSet
                .Include(q => q.Items)
                    .ThenInclude(i => i.RFQItem)
                        .ThenInclude(ri => ri!.Material)
                .Include(q => q.Documents)
                .Include(q => q.RFQ)
                    .ThenInclude(r => r!.Company)
                .Include(q => q.SupplierCompany)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Quotation>> GetByRfqAsync(int rfqId)
        {
            return await _dbSet
                .Include(q => q.SupplierCompany)
                .Include(q => q.Items)
                .Where(q => q.RfqId == rfqId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Quotation>> GetBySupplierAsync(int supplierCompanyId)
        {
            return await _dbSet
                .Include(q => q.RFQ)
                    .ThenInclude(r => r!.Company)
                .Include(q => q.Items)
                .Where(q => q.SupplierCompanyId == supplierCompanyId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<QuotationComparisonItem>> GetComparisonMatrixAsync(int rfqId)
        {
            // RFQ'ya ait tüm kalemleri getir
            var rfqItems = await _context.RFQItems
                .Include(ri => ri.Material)
                .Where(ri => ri.RfqId == rfqId)
                .OrderBy(ri => ri.Id)
                .ToListAsync();

            // Bu RFQ'ya ait tüm teklifleri ve kalemlerini getir
            var quotations = await _dbSet
                .Include(q => q.SupplierCompany)
                .Include(q => q.Items)
                .Where(q => q.RfqId == rfqId)
                .ToListAsync();

            // Karşılaştırma matrisini oluştur
            var result = new List<QuotationComparisonItem>();

            foreach (var rfqItem in rfqItems)
            {
                var comparisonItem = new QuotationComparisonItem
                {
                    RfqItem = rfqItem,
                    QuotationItems = new List<QuotationItemDetail>()
                };

                foreach (var quotation in quotations)
                {
                    var quotationItem = quotation.Items.FirstOrDefault(qi => qi.RfqItemId == rfqItem.Id);
                    if (quotationItem != null)
                    {
                        comparisonItem.QuotationItems.Add(new QuotationItemDetail
                        {
                            SupplierName = quotation.SupplierCompany?.CompanyName ?? "Bilinmiyor",
                            QuotationNumber = quotation.QuotationNumber,
                            UnitPrice = quotationItem.UnitPrice,
                            OfferedQuantity = quotationItem.OfferedQuantity,
                            TotalPrice = quotationItem.TotalPrice,
                            DeliveryDate = quotationItem.DeliveryDate,
                            ApprovalStatus = quotationItem.ApprovalStatus
                        });
                    }
                }

                // En düşük fiyata göre sırala
                comparisonItem.QuotationItems = comparisonItem.QuotationItems
                    .OrderBy(qi => qi.UnitPrice)
                    .ToList();

                result.Add(comparisonItem);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Quotation>> GetByStatusAsync(QuotationStatus status)
        {
            return await _dbSet
                .Include(q => q.RFQ)
                .Include(q => q.SupplierCompany)
                .Where(q => q.Status == status)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }
    }
}
