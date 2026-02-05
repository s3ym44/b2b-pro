using AutoMapper;
using B2BProcurement.Business.DTOs.Supplier;
using B2BProcurement.Business.Exceptions;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Core.Entities;
using B2BProcurement.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace B2BProcurement.Business.Services
{
    /// <summary>
    /// Tedarikçi servisi implementasyonu.
    /// </summary>
    public class SupplierService : ISupplierService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SupplierService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<SupplierDto?> GetByIdAsync(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Company)
                .Include(s => s.Sector)
                .FirstOrDefaultAsync(s => s.Id == id);

            return supplier == null ? null : _mapper.Map<SupplierDto>(supplier);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SupplierListDto>> GetAllAsync()
        {
            var suppliers = await _context.Suppliers
                .Include(s => s.Sector)
                .ToListAsync();

            return _mapper.Map<IEnumerable<SupplierListDto>>(suppliers);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SupplierListDto>> GetByCompanyAsync(int companyId)
        {
            var suppliers = await _context.Suppliers
                .Include(s => s.Sector)
                .Where(s => s.CompanyId == companyId)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return _mapper.Map<IEnumerable<SupplierListDto>>(suppliers);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SupplierListDto>> GetBySectorAsync(int sectorId)
        {
            var suppliers = await _context.Suppliers
                .Include(s => s.Sector)
                .Where(s => s.SectorId == sectorId)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return _mapper.Map<IEnumerable<SupplierListDto>>(suppliers);
        }

        /// <inheritdoc/>
        public async Task<SupplierDto> CreateAsync(int companyId, SupplierCreateDto dto)
        {
            var supplier = _mapper.Map<Supplier>(dto);
            supplier.CompanyId = companyId;

            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(supplier.Id) ?? throw new NotFoundException("Tedarikçi", supplier.Id);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(int id, SupplierUpdateDto dto)
        {
            var supplier = await _context.Suppliers.FindAsync(id)
                ?? throw new NotFoundException("Tedarikçi", id);

            _mapper.Map(dto, supplier);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id)
                ?? throw new NotFoundException("Tedarikçi", id);

            supplier.IsActive = false;
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SupplierListDto>> SearchAsync(int companyId, string query)
        {
            var queryable = _context.Suppliers
                .Include(s => s.Sector)
                .Where(s => s.CompanyId == companyId);

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                queryable = queryable.Where(s => 
                    s.Name.ToLower().Contains(query) ||
                    (s.Email != null && s.Email.ToLower().Contains(query)));
            }

            var suppliers = await queryable
                .OrderBy(s => s.Name)
                .Take(50)
                .ToListAsync();

            return _mapper.Map<IEnumerable<SupplierListDto>>(suppliers);
        }
    }
}
