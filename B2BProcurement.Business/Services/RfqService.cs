using AutoMapper;
using B2BProcurement.Business.DTOs.Rfq;
using B2BProcurement.Business.Exceptions;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Core.Entities;
using B2BProcurement.Core.Enums;
using B2BProcurement.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace B2BProcurement.Business.Services
{
    /// <summary>
    /// RFQ servisi implementasyonu.
    /// </summary>
    public class RfqService : IRfqService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICompanyService _companyService;
        private readonly INotificationService _notificationService;

        public RfqService(
            ApplicationDbContext context, 
            IMapper mapper, 
            ICompanyService companyService,
            INotificationService notificationService)
        {
            _context = context;
            _mapper = mapper;
            _companyService = companyService;
            _notificationService = notificationService;
        }

        /// <inheritdoc/>
        public async Task<RfqDto?> GetByIdAsync(int id)
        {
            var rfq = await _context.RFQs
                .Include(r => r.Company)
                .Include(r => r.Sector)
                .Include(r => r.Items)
                .Include(r => r.Quotations)
                .FirstOrDefaultAsync(r => r.Id == id);

            return rfq == null ? null : _mapper.Map<RfqDto>(rfq);
        }

        /// <inheritdoc/>
        public async Task<RfqDetailDto?> GetDetailAsync(int id)
        {
            var rfq = await _context.RFQs
                .Include(r => r.Company)
                .Include(r => r.Sector)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Material)
                .Include(r => r.Documents)
                .Include(r => r.Contacts)
                .FirstOrDefaultAsync(r => r.Id == id);

            return rfq == null ? null : _mapper.Map<RfqDetailDto>(rfq);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RfqListDto>> GetAllAsync()
        {
            var rfqs = await _context.RFQs
                .Include(r => r.Company)
                .Include(r => r.Sector)
                .Include(r => r.Items)
                .Include(r => r.Quotations)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RfqListDto>>(rfqs);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RfqListDto>> GetByCompanyAsync(int companyId)
        {
            var rfqs = await _context.RFQs
                .Include(r => r.Company)
                .Include(r => r.Sector)
                .Include(r => r.Items)
                .Include(r => r.Quotations)
                .Where(r => r.CompanyId == companyId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RfqListDto>>(rfqs);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RfqListDto>> GetBySectorAsync(int sectorId)
        {
            var rfqs = await _context.RFQs
                .Include(r => r.Company)
                .Include(r => r.Sector)
                .Include(r => r.Items)
                .Include(r => r.Quotations)
                .Where(r => r.SectorId == sectorId && r.Status == RfqStatus.Published)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RfqListDto>>(rfqs);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RfqListDto>> GetByStatusAsync(RfqStatus status)
        {
            var rfqs = await _context.RFQs
                .Include(r => r.Company)
                .Include(r => r.Sector)
                .Include(r => r.Items)
                .Include(r => r.Quotations)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RfqListDto>>(rfqs);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RfqListDto>> GetIncomingRfqsAsync(int companyId)
        {
            // Şirketin sektörünü bul
            var company = await _context.Companies.FindAsync(companyId)
                ?? throw new NotFoundException("Şirket", companyId);

            // Şirketin sektörüne göre yayınlanmış ve aktif RFQ'ları getir
            // Kendi şirketinin RFQ'larını hariç tut
            var rfqs = await _context.RFQs
                .Include(r => r.Company)
                .Include(r => r.Sector)
                .Include(r => r.Items)
                .Include(r => r.Quotations)
                .Where(r => r.SectorId == company.SectorId 
                    && r.Status == RfqStatus.Published 
                    && r.CompanyId != companyId
                    && r.EndDate > DateTime.Now)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RfqListDto>>(rfqs);
        }

        /// <inheritdoc/>
        public async Task<RfqDto> CreateAsync(int companyId, RfqCreateDto dto)
        {
            // Paket limiti kontrolü
            if (!await _companyService.CheckPackageLimitAsync(companyId, "rfq"))
            {
                throw new PackageLimitExceededException("rfq", 
                    "Aylık RFQ limitine ulaştınız. Paketinizi yükseltmeniz gerekiyor.");
            }

            // Tarih kontrolü
            if (dto.EndDate <= dto.StartDate)
            {
                throw new BusinessException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.", "INVALID_DATE");
            }

            var rfq = _mapper.Map<RFQ>(dto);
            rfq.CompanyId = companyId;
            rfq.RfqNumber = await GenerateRfqNumberAsync();
            rfq.Status = RfqStatus.Draft;

            await _context.RFQs.AddAsync(rfq);
            await _context.SaveChangesAsync();

            // Kalemleri ekle
            foreach (var itemDto in dto.Items)
            {
                var item = _mapper.Map<RFQItem>(itemDto);
                item.RfqId = rfq.Id;
                await _context.RFQItems.AddAsync(item);
            }

            // İletişim kişilerini ekle
            if (dto.Contacts != null)
            {
                foreach (var contactDto in dto.Contacts)
                {
                    var contact = _mapper.Map<RFQContact>(contactDto);
                    contact.RfqId = rfq.Id;
                    await _context.RFQContacts.AddAsync(contact);
                }
            }

            await _context.SaveChangesAsync();

            return await GetByIdAsync(rfq.Id) ?? throw new NotFoundException("RFQ", rfq.Id);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(int id, RfqUpdateDto dto)
        {
            var rfq = await _context.RFQs.FindAsync(id)
                ?? throw new NotFoundException("RFQ", id);

            // Sadece taslak durumundaki RFQ'lar güncellenebilir
            if (rfq.Status != RfqStatus.Draft)
            {
                throw new InvalidStateException(rfq.Status.ToString(), "Draft", 
                    "Sadece taslak durumundaki RFQ'lar güncellenebilir.");
            }

            _mapper.Map(dto, rfq);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id)
        {
            var rfq = await _context.RFQs.FindAsync(id)
                ?? throw new NotFoundException("RFQ", id);

            rfq.IsActive = false;
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<string> GenerateRfqNumberAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"RFQ-{year}-";

            // Bu yıla ait son RFQ numarasını bul
            var lastRfq = await _context.RFQs
                .Where(r => r.RfqNumber.StartsWith(prefix))
                .OrderByDescending(r => r.RfqNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastRfq != null)
            {
                var lastNumberStr = lastRfq.RfqNumber.Replace(prefix, "");
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D5}";
        }

        /// <inheritdoc/>
        public async Task PublishAsync(int rfqId)
        {
            var rfq = await _context.RFQs
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == rfqId)
                ?? throw new NotFoundException("RFQ", rfqId);

            // Durum kontrolü
            if (rfq.Status != RfqStatus.Draft)
            {
                throw new InvalidStateException(rfq.Status.ToString(), "Draft", 
                    "Sadece taslak durumundaki RFQ'lar yayınlanabilir.");
            }

            // Kalem kontrolü
            if (!rfq.Items.Any())
            {
                throw new BusinessException("RFQ'da en az bir kalem olmalıdır.", "NO_ITEMS");
            }

            // Tarih kontrolü
            if (rfq.EndDate <= DateTime.Now)
            {
                throw new BusinessException("Bitiş tarihi geçmiş bir tarih olamaz.", "INVALID_END_DATE");
            }

            rfq.Status = RfqStatus.Published;
            await _context.SaveChangesAsync();

            // Bildirim gönder
            await _notificationService.NotifyNewRfqAsync(rfqId);
        }

        /// <inheritdoc/>
        public async Task CloseAsync(int rfqId)
        {
            var rfq = await _context.RFQs.FindAsync(rfqId)
                ?? throw new NotFoundException("RFQ", rfqId);

            if (rfq.Status != RfqStatus.Published)
            {
                throw new InvalidStateException(rfq.Status.ToString(), "Published", 
                    "Sadece yayınlanmış RFQ'lar kapatılabilir.");
            }

            rfq.Status = RfqStatus.Closed;
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task CancelAsync(int rfqId)
        {
            var rfq = await _context.RFQs.FindAsync(rfqId)
                ?? throw new NotFoundException("RFQ", rfqId);

            if (rfq.Status == RfqStatus.Completed)
            {
                throw new InvalidStateException(rfq.Status.ToString(), "NotCompleted", 
                    "Tamamlanmış RFQ'lar iptal edilemez.");
            }

            rfq.Status = RfqStatus.Cancelled;
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<int> CloseExpiredRfqsAsync()
        {
            var expiredRfqs = await _context.RFQs
                .Where(r => r.Status == RfqStatus.Published && r.EndDate < DateTime.Now)
                .ToListAsync();

            foreach (var rfq in expiredRfqs)
            {
                rfq.Status = RfqStatus.Closed;
            }

            await _context.SaveChangesAsync();
            return expiredRfqs.Count;
        }

        /// <inheritdoc/>
        public async Task AddItemAsync(int rfqId, RfqItemCreateDto itemDto)
        {
            var rfq = await _context.RFQs.FindAsync(rfqId)
                ?? throw new NotFoundException("RFQ", rfqId);

            if (rfq.Status != RfqStatus.Draft)
            {
                throw new InvalidStateException(rfq.Status.ToString(), "Draft", 
                    "Sadece taslak durumundaki RFQ'lara kalem eklenebilir.");
            }

            var item = _mapper.Map<RFQItem>(itemDto);
            item.RfqId = rfqId;
            await _context.RFQItems.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateItemAsync(int itemId, RfqItemCreateDto itemDto)
        {
            var item = await _context.RFQItems
                .Include(i => i.RFQ)
                .FirstOrDefaultAsync(i => i.Id == itemId)
                ?? throw new NotFoundException("RFQ Kalemi", itemId);

            if (item.RFQ!.Status != RfqStatus.Draft)
            {
                throw new InvalidStateException(item.RFQ.Status.ToString(), "Draft", 
                    "Sadece taslak durumundaki RFQ kalemleri güncellenebilir.");
            }

            _mapper.Map(itemDto, item);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteItemAsync(int itemId)
        {
            var item = await _context.RFQItems
                .Include(i => i.RFQ)
                .FirstOrDefaultAsync(i => i.Id == itemId)
                ?? throw new NotFoundException("RFQ Kalemi", itemId);

            if (item.RFQ!.Status != RfqStatus.Draft)
            {
                throw new InvalidStateException(item.RFQ.Status.ToString(), "Draft", 
                    "Sadece taslak durumundaki RFQ kalemleri silinebilir.");
            }

            item.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
