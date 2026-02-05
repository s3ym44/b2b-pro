using AutoMapper;
using B2BProcurement.Business.DTOs.Quotation;
using B2BProcurement.Business.Exceptions;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Core.Entities;
using B2BProcurement.Core.Enums;
using B2BProcurement.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace B2BProcurement.Business.Services
{
    /// <summary>
    /// Teklif servisi implementasyonu.
    /// </summary>
    public class QuotationService : IQuotationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public QuotationService(
            ApplicationDbContext context, 
            IMapper mapper,
            INotificationService notificationService)
        {
            _context = context;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        /// <inheritdoc/>
        public async Task<QuotationDto?> GetByIdAsync(int id)
        {
            var quotation = await _context.Quotations
                .Include(q => q.RFQ)
                .Include(q => q.SupplierCompany)
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == id);

            return quotation == null ? null : _mapper.Map<QuotationDto>(quotation);
        }

        /// <inheritdoc/>
        public async Task<QuotationDto?> GetDetailAsync(int id)
        {
            var quotation = await _context.Quotations
                .Include(q => q.RFQ)
                    .ThenInclude(r => r!.Company)
                .Include(q => q.SupplierCompany)
                .Include(q => q.Items)
                    .ThenInclude(i => i.RFQItem)
                .Include(q => q.Documents)
                .FirstOrDefaultAsync(q => q.Id == id);

            return quotation == null ? null : _mapper.Map<QuotationDto>(quotation);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<QuotationListDto>> GetAllAsync()
        {
            var quotations = await _context.Quotations
                .Include(q => q.RFQ)
                .Include(q => q.SupplierCompany)
                .Include(q => q.Items)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<QuotationListDto>>(quotations);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<QuotationListDto>> GetByRfqAsync(int rfqId)
        {
            var quotations = await _context.Quotations
                .Include(q => q.RFQ)
                .Include(q => q.SupplierCompany)
                .Include(q => q.Items)
                .Where(q => q.RfqId == rfqId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<QuotationListDto>>(quotations);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<QuotationListDto>> GetByCompanyAsync(int companyId)
        {
            var quotations = await _context.Quotations
                .Include(q => q.RFQ)
                .Include(q => q.SupplierCompany)
                .Include(q => q.Items)
                .Where(q => q.SupplierCompanyId == companyId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<QuotationListDto>>(quotations);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<QuotationListDto>> GetByStatusAsync(QuotationStatus status)
        {
            var quotations = await _context.Quotations
                .Include(q => q.RFQ)
                .Include(q => q.SupplierCompany)
                .Include(q => q.Items)
                .Where(q => q.Status == status)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<QuotationListDto>>(quotations);
        }

        /// <inheritdoc/>
        public async Task<QuotationDto> CreateAsync(int companyId, QuotationCreateDto dto)
        {
            // RFQ kontrolü
            var rfq = await _context.RFQs
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == dto.RfqId)
                ?? throw new NotFoundException("RFQ", dto.RfqId);

            // RFQ durumu kontrolü - Sadece Published RFQ'lara teklif verilebilir
            if (rfq.Status != RfqStatus.Published)
            {
                throw new InvalidStateException(rfq.Status.ToString(), "Published", 
                    "Sadece yayınlanmış RFQ'lara teklif verilebilir.");
            }

            // Kendi RFQ'na teklif vermeme kontrolü
            if (rfq.CompanyId == companyId)
            {
                throw new BusinessException("Kendi RFQ'nuza teklif veremezsiniz.", "SELF_QUOTATION");
            }

            // Süre kontrolü
            if (rfq.EndDate < DateTime.Now)
            {
                throw new BusinessException("RFQ süresi dolmuş.", "RFQ_EXPIRED");
            }

            // Aynı şirketten daha önce teklif verilmiş mi?
            var existingQuotation = await _context.Quotations
                .FirstOrDefaultAsync(q => q.RfqId == dto.RfqId && q.SupplierCompanyId == companyId);
            
            if (existingQuotation != null)
            {
                throw new BusinessException("Bu RFQ'ya zaten teklif verdiniz.", "ALREADY_QUOTED");
            }

            var quotation = new Quotation
            {
                QuotationNumber = await GenerateQuotationNumberAsync(),
                RfqId = dto.RfqId,
                SupplierCompanyId = companyId,
                Status = QuotationStatus.Draft,
                ValidUntil = dto.ValidUntil,
                TotalAmount = 0
            };

            await _context.Quotations.AddAsync(quotation);
            await _context.SaveChangesAsync();

            // Kalemleri ekle
            decimal totalAmount = 0;
            foreach (var itemDto in dto.Items)
            {
                // RFQ kalemi kontrolü
                var rfqItem = rfq.Items.FirstOrDefault(i => i.Id == itemDto.RfqItemId)
                    ?? throw new NotFoundException("RFQ Kalemi", itemDto.RfqItemId);

                var totalPrice = itemDto.UnitPrice * itemDto.OfferedQuantity;
                var item = new QuotationItem
                {
                    QuotationId = quotation.Id,
                    RfqItemId = itemDto.RfqItemId,
                    UnitPrice = itemDto.UnitPrice,
                    OfferedQuantity = itemDto.OfferedQuantity,
                    TotalPrice = totalPrice,
                    DeliveryDate = itemDto.DeliveryDate,
                    ApprovalStatus = ApprovalStatus.Pending
                };

                await _context.QuotationItems.AddAsync(item);
                totalAmount += totalPrice;
            }

            quotation.TotalAmount = totalAmount;
            await _context.SaveChangesAsync();

            return await GetByIdAsync(quotation.Id) ?? throw new NotFoundException("Teklif", quotation.Id);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(int id, QuotationCreateDto dto)
        {
            var quotation = await _context.Quotations.FindAsync(id)
                ?? throw new NotFoundException("Teklif", id);

            if (quotation.Status != QuotationStatus.Draft)
            {
                throw new InvalidStateException(quotation.Status.ToString(), "Draft", 
                    "Sadece taslak durumundaki teklifler güncellenebilir.");
            }

            quotation.ValidUntil = dto.ValidUntil;
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id)
        {
            var quotation = await _context.Quotations.FindAsync(id)
                ?? throw new NotFoundException("Teklif", id);

            quotation.IsActive = false;
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<string> GenerateQuotationNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"QUO-{year}-";

            var lastQuotation = await _context.Quotations
                .Where(q => q.QuotationNumber.StartsWith(prefix))
                .OrderByDescending(q => q.QuotationNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastQuotation != null)
            {
                var lastNumberStr = lastQuotation.QuotationNumber.Replace(prefix, "");
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D5}";
        }

        /// <inheritdoc/>
        public async Task SubmitAsync(int quotationId)
        {
            var quotation = await _context.Quotations
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == quotationId)
                ?? throw new NotFoundException("Teklif", quotationId);

            if (quotation.Status != QuotationStatus.Draft)
            {
                throw new InvalidStateException(quotation.Status.ToString(), "Draft", 
                    "Sadece taslak durumundaki teklifler gönderilebilir.");
            }

            if (!quotation.Items.Any())
            {
                throw new BusinessException("Teklifte en az bir kalem olmalıdır.", "NO_ITEMS");
            }

            quotation.Status = QuotationStatus.Submitted;
            await _context.SaveChangesAsync();

            // Bildirim gönder
            await _notificationService.NotifyNewQuotationAsync(quotationId);
        }

        /// <inheritdoc/>
        public async Task WithdrawAsync(int quotationId)
        {
            var quotation = await _context.Quotations.FindAsync(quotationId)
                ?? throw new NotFoundException("Teklif", quotationId);

            if (quotation.Status != QuotationStatus.Submitted)
            {
                throw new InvalidStateException(quotation.Status.ToString(), "Submitted", 
                    "Sadece gönderilmiş teklifler geri çekilebilir.");
            }

            quotation.Status = QuotationStatus.Withdrawn;
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<QuotationComparisonDto> GetComparisonMatrixAsync(int rfqId)
        {
            var rfq = await _context.RFQs
                .Include(r => r.Items)
                    .ThenInclude(i => i.Material)
                .FirstOrDefaultAsync(r => r.Id == rfqId)
                ?? throw new NotFoundException("RFQ", rfqId);

            var quotations = await _context.Quotations
                .Include(q => q.SupplierCompany)
                .Include(q => q.Items)
                .Where(q => q.RfqId == rfqId && q.Status == QuotationStatus.Submitted)
                .ToListAsync();

            var result = new QuotationComparisonDto
            {
                RfqId = rfqId,
                RfqNumber = rfq.RfqNumber,
                RfqTitle = rfq.Title,
                Suppliers = quotations.Select(q => new ComparisonSupplierDto
                {
                    CompanyId = q.SupplierCompanyId,
                    CompanyName = q.SupplierCompany?.CompanyName ?? "",
                    QuotationNumber = q.QuotationNumber,
                    TotalAmount = q.TotalAmount
                }).ToList(),
                Items = new List<ComparisonItemDto>()
            };

            foreach (var rfqItem in rfq.Items.Where(i => i.IsActive))
            {
                var comparisonItem = new ComparisonItemDto
                {
                    RfqItemId = rfqItem.Id,
                    Description = rfqItem.Description,
                    RequestedQuantity = rfqItem.Quantity,
                    Unit = rfqItem.Unit,
                    Prices = new List<ComparisonPriceDto>()
                };

                // Her teklifteki bu kaleme ait fiyatları bul
                var prices = new List<ComparisonPriceDto>();
                foreach (var quotation in quotations)
                {
                    var quotationItem = quotation.Items.FirstOrDefault(qi => qi.RfqItemId == rfqItem.Id);
                    if (quotationItem != null)
                    {
                        prices.Add(new ComparisonPriceDto
                        {
                            SupplierCompanyId = quotation.SupplierCompanyId,
                            SupplierName = quotation.SupplierCompany?.CompanyName,
                            UnitPrice = quotationItem.UnitPrice,
                            OfferedQuantity = quotationItem.OfferedQuantity,
                            TotalPrice = quotationItem.TotalPrice,
                            DeliveryDate = quotationItem.DeliveryDate,
                            ApprovalStatus = quotationItem.ApprovalStatus,
                            IsLowestPrice = false
                        });
                    }
                }

                // En düşük fiyatı işaretle
                if (prices.Any())
                {
                    var minPrice = prices.Min(p => p.UnitPrice);
                    foreach (var price in prices.Where(p => p.UnitPrice == minPrice))
                    {
                        price.IsLowestPrice = true;
                    }
                }

                comparisonItem.Prices = prices.OrderBy(p => p.UnitPrice).ToList();
                result.Items.Add(comparisonItem);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task ApproveItemAsync(int quotationItemId, decimal? approvedQuantity = null)
        {
            var item = await _context.QuotationItems
                .Include(i => i.Quotation)
                .FirstOrDefaultAsync(i => i.Id == quotationItemId)
                ?? throw new NotFoundException("Teklif Kalemi", quotationItemId);

            item.ApprovalStatus = ApprovalStatus.Approved;
            item.ApprovedQuantity = approvedQuantity ?? item.OfferedQuantity;
            await _context.SaveChangesAsync();

            // Tüm kalemler onaylandıysa teklifi onayla
            var allItems = await _context.QuotationItems
                .Where(i => i.QuotationId == item.QuotationId)
                .ToListAsync();

            if (allItems.All(i => i.ApprovalStatus == ApprovalStatus.Approved))
            {
                item.Quotation!.Status = QuotationStatus.Approved;
                await _context.SaveChangesAsync();
                await _notificationService.NotifyQuotationApprovedAsync(item.QuotationId);
            }
        }

        /// <inheritdoc/>
        public async Task RejectItemAsync(int quotationItemId, string? reason = null)
        {
            var item = await _context.QuotationItems
                .Include(i => i.Quotation)
                .FirstOrDefaultAsync(i => i.Id == quotationItemId)
                ?? throw new NotFoundException("Teklif Kalemi", quotationItemId);

            item.ApprovalStatus = ApprovalStatus.Rejected;
            await _context.SaveChangesAsync();

            // Tüm kalemler reddedildiyse teklifi reddet
            var allItems = await _context.QuotationItems
                .Where(i => i.QuotationId == item.QuotationId)
                .ToListAsync();

            if (allItems.All(i => i.ApprovalStatus == ApprovalStatus.Rejected))
            {
                item.Quotation!.Status = QuotationStatus.Rejected;
                await _context.SaveChangesAsync();
                await _notificationService.NotifyQuotationRejectedAsync(item.QuotationId);
            }
        }

        /// <inheritdoc/>
        public async Task ApproveAllItemsAsync(int quotationId)
        {
            var items = await _context.QuotationItems
                .Where(i => i.QuotationId == quotationId)
                .ToListAsync();

            foreach (var item in items)
            {
                item.ApprovalStatus = ApprovalStatus.Approved;
                item.ApprovedQuantity = item.OfferedQuantity;
            }

            var quotation = await _context.Quotations.FindAsync(quotationId)
                ?? throw new NotFoundException("Teklif", quotationId);

            quotation.Status = QuotationStatus.Approved;
            await _context.SaveChangesAsync();
            await _notificationService.NotifyQuotationApprovedAsync(quotationId);
        }

        /// <inheritdoc/>
        public async Task FinalizeAsync(int quotationId)
        {
            var quotation = await _context.Quotations.FindAsync(quotationId)
                ?? throw new NotFoundException("Teklif", quotationId);

            if (quotation.Status != QuotationStatus.Approved)
            {
                throw new InvalidStateException(quotation.Status.ToString(), "Approved", 
                    "Sadece onaylanmış teklifler tamamlanabilir.");
            }

            quotation.Status = QuotationStatus.Completed;
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task AddItemAsync(int quotationId, QuotationItemCreateDto itemDto)
        {
            var quotation = await _context.Quotations.FindAsync(quotationId)
                ?? throw new NotFoundException("Teklif", quotationId);

            if (quotation.Status != QuotationStatus.Draft)
            {
                throw new InvalidStateException(quotation.Status.ToString(), "Draft", 
                    "Sadece taslak durumundaki tekliflere kalem eklenebilir.");
            }

            var item = _mapper.Map<QuotationItem>(itemDto);
            item.QuotationId = quotationId;
            item.TotalPrice = item.UnitPrice * item.OfferedQuantity;

            await _context.QuotationItems.AddAsync(item);
            await RecalculateTotalAsync(quotationId);
        }

        /// <inheritdoc/>
        public async Task UpdateItemAsync(int itemId, QuotationItemCreateDto itemDto)
        {
            var item = await _context.QuotationItems
                .Include(i => i.Quotation)
                .FirstOrDefaultAsync(i => i.Id == itemId)
                ?? throw new NotFoundException("Teklif Kalemi", itemId);

            if (item.Quotation!.Status != QuotationStatus.Draft)
            {
                throw new InvalidStateException(item.Quotation.Status.ToString(), "Draft", 
                    "Sadece taslak durumundaki teklif kalemleri güncellenebilir.");
            }

            item.UnitPrice = itemDto.UnitPrice;
            item.OfferedQuantity = itemDto.OfferedQuantity;
            item.DeliveryDate = itemDto.DeliveryDate;
            item.TotalPrice = item.UnitPrice * item.OfferedQuantity;

            await _context.SaveChangesAsync();
            await RecalculateTotalAsync(item.QuotationId);
        }

        /// <inheritdoc/>
        public async Task DeleteItemAsync(int itemId)
        {
            var item = await _context.QuotationItems
                .Include(i => i.Quotation)
                .FirstOrDefaultAsync(i => i.Id == itemId)
                ?? throw new NotFoundException("Teklif Kalemi", itemId);

            if (item.Quotation!.Status != QuotationStatus.Draft)
            {
                throw new InvalidStateException(item.Quotation.Status.ToString(), "Draft", 
                    "Sadece taslak durumundaki teklif kalemleri silinebilir.");
            }

            item.IsActive = false;
            await _context.SaveChangesAsync();
            await RecalculateTotalAsync(item.QuotationId);
        }

        /// <inheritdoc/>
        public async Task RecalculateTotalAsync(int quotationId)
        {
            var quotation = await _context.Quotations
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == quotationId)
                ?? throw new NotFoundException("Teklif", quotationId);

            quotation.TotalAmount = quotation.Items
                .Where(i => i.IsActive)
                .Sum(i => i.TotalPrice);

            await _context.SaveChangesAsync();
        }
    }
}
