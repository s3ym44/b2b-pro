using AutoMapper;
using B2BProcurement.Business.DTOs.Material;
using B2BProcurement.Business.Exceptions;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Core.Entities;
using B2BProcurement.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace B2BProcurement.Business.Services
{
    /// <summary>
    /// Malzeme servisi implementasyonu.
    /// </summary>
    public class MaterialService : IMaterialService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICompanyService _companyService;

        public MaterialService(ApplicationDbContext context, IMapper mapper, ICompanyService companyService)
        {
            _context = context;
            _mapper = mapper;
            _companyService = companyService;
        }

        /// <inheritdoc/>
        public async Task<MaterialDto?> GetByIdAsync(int id)
        {
            var material = await _context.Materials
                .Include(m => m.Company)
                .Include(m => m.Sector)
                .FirstOrDefaultAsync(m => m.Id == id);

            return material == null ? null : _mapper.Map<MaterialDto>(material);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MaterialListDto>> GetAllAsync()
        {
            var materials = await _context.Materials
                .Include(m => m.Sector)
                .ToListAsync();

            return _mapper.Map<IEnumerable<MaterialListDto>>(materials);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MaterialListDto>> GetByCompanyAsync(int companyId)
        {
            var materials = await _context.Materials
                .Include(m => m.Sector)
                .Where(m => m.CompanyId == companyId)
                .OrderBy(m => m.Code)
                .ToListAsync();

            return _mapper.Map<IEnumerable<MaterialListDto>>(materials);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MaterialListDto>> GetBySectorAsync(int sectorId)
        {
            var materials = await _context.Materials
                .Include(m => m.Sector)
                .Where(m => m.SectorId == sectorId)
                .OrderBy(m => m.Name)
                .ToListAsync();

            return _mapper.Map<IEnumerable<MaterialListDto>>(materials);
        }

        /// <inheritdoc/>
        public async Task<MaterialDto> CreateAsync(int companyId, MaterialCreateDto dto)
        {
            // Paket limiti kontrolü
            if (!await _companyService.CheckPackageLimitAsync(companyId, "materials"))
            {
                throw new PackageLimitExceededException("materials", 
                    "Malzeme limitine ulaştınız. Paketinizi yükseltmeniz gerekiyor.");
            }

            // Kod benzersizlik kontrolü
            if (!await IsCodeUniqueAsync(companyId, dto.Code))
            {
                throw new BusinessException("Bu malzeme kodu zaten kullanılıyor.", "CODE_EXISTS");
            }

            var material = _mapper.Map<Material>(dto);
            material.CompanyId = companyId;

            await _context.Materials.AddAsync(material);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(material.Id) ?? throw new NotFoundException("Malzeme", material.Id);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(int id, MaterialUpdateDto dto)
        {
            var material = await _context.Materials.FindAsync(id)
                ?? throw new NotFoundException("Malzeme", id);

            // Kod benzersizlik kontrolü
            if (!await IsCodeUniqueAsync(material.CompanyId, dto.Code, id))
            {
                throw new BusinessException("Bu malzeme kodu zaten kullanılıyor.", "CODE_EXISTS");
            }

            _mapper.Map(dto, material);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id)
        {
            var material = await _context.Materials.FindAsync(id)
                ?? throw new NotFoundException("Malzeme", id);

            material.IsActive = false;
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MaterialListDto>> SearchPublicAsync(string query, int? sectorId = null)
        {
            var queryable = _context.Materials
                .Include(m => m.Sector)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                queryable = queryable.Where(m => 
                    m.Name.ToLower().Contains(query) || 
                    m.Code.ToLower().Contains(query));
            }

            if (sectorId.HasValue)
            {
                queryable = queryable.Where(m => m.SectorId == sectorId.Value);
            }

            var materials = await queryable
                .OrderBy(m => m.Name)
                .Take(50)
                .ToListAsync();

            return _mapper.Map<IEnumerable<MaterialListDto>>(materials);
        }

        /// <inheritdoc/>
        public async Task<bool> IsCodeUniqueAsync(int companyId, string code, int? excludeId = null)
        {
            var query = _context.Materials
                .Where(m => m.CompanyId == companyId && m.Code == code);

            if (excludeId.HasValue)
            {
                query = query.Where(m => m.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}
