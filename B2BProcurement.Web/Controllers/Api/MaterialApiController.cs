using Microsoft.AspNetCore.Mvc;
using B2BProcurement.Attributes;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Business.DTOs.Material;

namespace B2BProcurement.Controllers.Api
{
    /// <summary>
    /// Material API Controller for external integrations.
    /// Provides material search and listing capabilities.
    /// </summary>
    [Route("api/material")]
    [ApiController]
    [ApiKeyAuth]
    [RateLimit(MaxRequests = 100, WindowSeconds = 60)]
    [Produces("application/json")]
    public class MaterialApiController : ControllerBase
    {
        private readonly IMaterialService _materialService;
        private readonly ILogger<MaterialApiController> _logger;

        public MaterialApiController(IMaterialService materialService, ILogger<MaterialApiController> logger)
        {
            _materialService = materialService;
            _logger = logger;
        }

        /// <summary>
        /// Search materials across the platform (public materials only).
        /// </summary>
        /// <param name="query">Search query (searches in code, name, description)</param>
        /// <param name="sectorId">Optional sector filter</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<MaterialApiDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Search(
            [FromQuery] string? query = null,
            [FromQuery] int? sectorId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                pageSize = Math.Min(pageSize, 100);
                page = Math.Max(page, 1);

                var materials = await _materialService.SearchPublicAsync(query ?? "", sectorId);

                var total = materials.Count();
                var items = materials
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m => new MaterialApiDto
                    {
                        Id = m.Id,
                        Code = m.Code,
                        Name = m.Name,
                        Unit = m.Unit,
                        SectorName = m.SectorName
                    })
                    .ToList();

                _logger.LogInformation("Material search via API: query='{Query}', sectorId={SectorId}, results={Count}", 
                    query, sectorId, total);

                return Ok(new ApiResponse<PagedResult<MaterialApiDto>>
                {
                    Success = true,
                    Data = new PagedResult<MaterialApiDto>
                    {
                        Items = items,
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = total,
                        TotalPages = (int)Math.Ceiling((double)total / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching materials");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = "An error occurred while searching materials",
                    Code = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Get materials by company ID.
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        [HttpGet("by-company/{companyId}")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<MaterialApiDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetByCompany(
            int companyId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                pageSize = Math.Min(pageSize, 100);
                page = Math.Max(page, 1);

                var materials = await _materialService.GetByCompanyAsync(companyId);

                if (!materials.Any())
                {
                    // Return empty list instead of 404 - company might exist but have no materials
                    return Ok(new ApiResponse<PagedResult<MaterialApiDto>>
                    {
                        Success = true,
                        Data = new PagedResult<MaterialApiDto>
                        {
                            Items = new List<MaterialApiDto>(),
                            Page = page,
                            PageSize = pageSize,
                            TotalCount = 0,
                            TotalPages = 0
                        }
                    });
                }

                var total = materials.Count();
                var items = materials
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m => new MaterialApiDto
                    {
                        Id = m.Id,
                        Code = m.Code,
                        Name = m.Name,
                        Unit = m.Unit,
                        SectorName = m.SectorName
                    })
                    .ToList();

                _logger.LogInformation("Materials retrieved for company {CompanyId} via API: {Count} items", 
                    companyId, total);

                return Ok(new ApiResponse<PagedResult<MaterialApiDto>>
                {
                    Success = true,
                    Data = new PagedResult<MaterialApiDto>
                    {
                        Items = items,
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = total,
                        TotalPages = (int)Math.Ceiling((double)total / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting materials for company {CompanyId}", companyId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = "An error occurred while retrieving materials",
                    Code = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Get material details by ID.
        /// </summary>
        /// <param name="id">Material ID</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<MaterialDetailApiDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var material = await _materialService.GetByIdAsync(id);
                if (material == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "Material not found",
                        Code = "MATERIAL_NOT_FOUND"
                    });
                }

                var result = new MaterialDetailApiDto
                {
                    Id = material.Id,
                    Code = material.Code,
                    Name = material.Name,
                    Description = material.Description,
                    Unit = material.Unit,
                    CompanyId = material.CompanyId,
                    CompanyName = material.CompanyName,
                    SectorId = material.SectorId,
                    SectorName = material.SectorName,
                    IsActive = material.IsActive,
                    CreatedAt = material.CreatedAt
                };

                return Ok(new ApiResponse<MaterialDetailApiDto>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting material {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = "An error occurred while retrieving material",
                    Code = "INTERNAL_ERROR"
                });
            }
        }
    }

    #region Material API DTOs

    /// <summary>
    /// Material list item DTO for API responses.
    /// </summary>
    public class MaterialApiDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Unit { get; set; } = "";
        public string? SectorName { get; set; }
    }

    /// <summary>
    /// Material detail DTO for API responses.
    /// </summary>
    public class MaterialDetailApiDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string Unit { get; set; } = "";
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int SectorId { get; set; }
        public string? SectorName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion
}
