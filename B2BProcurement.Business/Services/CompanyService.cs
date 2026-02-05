using AutoMapper;
using B2BProcurement.Business.DTOs.Company;
using B2BProcurement.Business.Exceptions;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Core.Entities;
using B2BProcurement.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace B2BProcurement.Business.Services
{
    /// <summary>
    /// Şirket servisi implementasyonu.
    /// </summary>
    public class CompanyService : ICompanyService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CompanyService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<CompanyDto?> GetByIdAsync(int id)
        {
            var company = await _context.Companies
                .Include(c => c.Sector)
                .Include(c => c.Package)
                .FirstOrDefaultAsync(c => c.Id == id);

            return company == null ? null : _mapper.Map<CompanyDto>(company);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CompanyListDto>> GetAllAsync()
        {
            var companies = await _context.Companies
                .Include(c => c.Sector)
                .Include(c => c.Package)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CompanyListDto>>(companies);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CompanyListDto>> GetBySectorAsync(int sectorId)
        {
            var companies = await _context.Companies
                .Include(c => c.Sector)
                .Include(c => c.Package)
                .Where(c => c.SectorId == sectorId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CompanyListDto>>(companies);
        }

        /// <inheritdoc/>
        public async Task<CompanyDto> CreateAsync(CompanyCreateDto dto)
        {
            // Vergi numarası kontrolü
            if (await ExistsByTaxNumberAsync(dto.TaxNumber))
            {
                throw new BusinessException("Bu vergi numarası zaten kayıtlı.", "TAX_NUMBER_EXISTS");
            }

            var company = _mapper.Map<Company>(dto);
            await _context.Companies.AddAsync(company);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(company.Id) ?? throw new NotFoundException("Şirket", company.Id);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(int id, CompanyUpdateDto dto)
        {
            var company = await _context.Companies.FindAsync(id)
                ?? throw new NotFoundException("Şirket", id);

            _mapper.Map(dto, company);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id)
        {
            var company = await _context.Companies.FindAsync(id)
                ?? throw new NotFoundException("Şirket", id);

            company.IsActive = false;
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> CheckPackageLimitAsync(int companyId, string limitType)
        {
            var company = await _context.Companies
                .Include(c => c.Package)
                .FirstOrDefaultAsync(c => c.Id == companyId)
                ?? throw new NotFoundException("Şirket", companyId);

            var package = company.Package ?? throw new NotFoundException("Paket", company.PackageId);

            switch (limitType.ToLower())
            {
                case "users":
                    if (package.MaxUsers == 0) return true; // Sınırsız
                    var userCount = await _context.Users.CountAsync(u => u.CompanyId == companyId);
                    return userCount < package.MaxUsers;

                case "materials":
                    if (package.MaxMaterials == 0) return true;
                    var materialCount = await _context.Materials.CountAsync(m => m.CompanyId == companyId);
                    return materialCount < package.MaxMaterials;

                case "rfq":
                    if (package.MaxRfqPerMonth == 0) return true;
                    var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    var rfqCount = await _context.RFQs
                        .CountAsync(r => r.CompanyId == companyId && r.CreatedAt >= startOfMonth);
                    return rfqCount < package.MaxRfqPerMonth;

                default:
                    return true;
            }
        }

        /// <inheritdoc/>
        public async Task<PackageUsageDto> GetPackageUsageAsync(int companyId)
        {
            var company = await _context.Companies
                .Include(c => c.Package)
                .FirstOrDefaultAsync(c => c.Id == companyId)
                ?? throw new NotFoundException("Şirket", companyId);

            var package = company.Package ?? throw new NotFoundException("Paket", company.PackageId);

            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            return new PackageUsageDto
            {
                PackageName = package.Name,
                MaxUsers = package.MaxUsers,
                CurrentUsers = await _context.Users.CountAsync(u => u.CompanyId == companyId),
                MaxMaterials = package.MaxMaterials,
                CurrentMaterials = await _context.Materials.CountAsync(m => m.CompanyId == companyId),
                MaxRfqPerMonth = package.MaxRfqPerMonth,
                CurrentRfqThisMonth = await _context.RFQs
                    .CountAsync(r => r.CompanyId == companyId && r.CreatedAt >= startOfMonth),
                CanUseSapIntegration = package.CanUseSapIntegration
            };
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsByTaxNumberAsync(string taxNumber)
        {
            return await _context.Companies.AnyAsync(c => c.TaxNumber == taxNumber);
        }
    }
}
