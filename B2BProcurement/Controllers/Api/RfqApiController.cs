using Microsoft.AspNetCore.Mvc;
using B2BProcurement.Attributes;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Business.DTOs.Rfq;
using B2BProcurement.Core.Enums;

namespace B2BProcurement.Controllers.Api
{
    /// <summary>
    /// RFQ API Controller for external integrations (SAP, ERP systems).
    /// </summary>
    [Route("api/rfq")]
    [ApiController]
    [ApiKeyAuth]
    [RateLimit(MaxRequests = 100, WindowSeconds = 60)]
    [Produces("application/json")]
    public class RfqApiController : ControllerBase
    {
        private readonly IRfqService _rfqService;
        private readonly ILogger<RfqApiController> _logger;

        public RfqApiController(IRfqService rfqService, ILogger<RfqApiController> logger)
        {
            _rfqService = rfqService;
            _logger = logger;
        }

        /// <summary>
        /// Get all RFQs with optional filtering.
        /// </summary>
        /// <param name="status">Filter by status (Draft, Published, Closed, Cancelled)</param>
        /// <param name="sectorId">Filter by sector ID</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RfqListApiDto>>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] RfqStatus? status = null,
            [FromQuery] int? sectorId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                pageSize = Math.Min(pageSize, 100);
                page = Math.Max(page, 1);

                IEnumerable<RfqListDto> rfqs;

                if (status.HasValue)
                {
                    rfqs = await _rfqService.GetByStatusAsync(status.Value);
                }
                else if (sectorId.HasValue)
                {
                    rfqs = await _rfqService.GetBySectorAsync(sectorId.Value);
                }
                else
                {
                    rfqs = await _rfqService.GetAllAsync();
                }

                var total = rfqs.Count();
                var items = rfqs
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RfqListApiDto
                    {
                        Id = r.Id,
                        RfqNumber = r.RfqNumber,
                        Title = r.Title,
                        CompanyName = r.CompanyName,
                        SectorName = r.SectorName,
                        Status = r.Status.ToString(),
                        EndDate = r.EndDate,
                        ItemCount = r.ItemCount,
                        QuotationCount = r.QuotationCount
                    })
                    .ToList();

                return Ok(new ApiResponse<PagedResult<RfqListApiDto>>
                {
                    Success = true,
                    Data = new PagedResult<RfqListApiDto>
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
                _logger.LogError(ex, "Error getting RFQs");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = "An error occurred while retrieving RFQs",
                    Code = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Get RFQ details by ID.
        /// </summary>
        /// <param name="id">RFQ ID</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RfqDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var rfq = await _rfqService.GetByIdAsync(id);
                if (rfq == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "RFQ not found",
                        Code = "RFQ_NOT_FOUND"
                    });
                }

                return Ok(new ApiResponse<RfqDto>
                {
                    Success = true,
                    Data = rfq
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting RFQ {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = "An error occurred while retrieving RFQ",
                    Code = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Create a new RFQ (e.g., from SAP/ERP system).
        /// </summary>
        /// <param name="request">RFQ creation request</param>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RfqCreateResult>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Create([FromBody] RfqCreateApiRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "Invalid request data",
                        Code = "VALIDATION_ERROR",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                // Map to RfqCreateDto
                var createDto = new RfqCreateDto
                {
                    Title = request.Title,
                    SectorId = request.SectorId,
                    Visibility = request.Visibility,
                    StartDate = request.StartDate ?? DateTime.Now,
                    EndDate = request.EndDate,
                    Currency = request.Currency ?? "TRY",
                    Items = request.Items?.Select(i => new RfqItemCreateDto
                    {
                        MaterialId = i.MaterialId,
                        Description = i.Description ?? "",
                        Quantity = i.Quantity,
                        Unit = i.Unit ?? "Adet",
                        TechnicalSpecs = i.TechnicalSpecs
                    }).ToList() ?? new List<RfqItemCreateDto>()
                };

                var result = await _rfqService.CreateAsync(request.CompanyId, createDto);

                _logger.LogInformation("RFQ created via API: {RfqNumber}", result.RfqNumber);

                return CreatedAtAction(nameof(GetById), new { id = result.Id }, new ApiResponse<RfqCreateResult>
                {
                    Success = true,
                    Data = new RfqCreateResult
                    {
                        Id = result.Id,
                        RfqNumber = result.RfqNumber,
                        Status = result.Status.ToString(),
                        Message = "RFQ created successfully"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating RFQ");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = "An error occurred while creating RFQ",
                    Code = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Update RFQ status.
        /// </summary>
        /// <param name="id">RFQ ID</param>
        /// <param name="request">Status update request</param>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateRequest request)
        {
            try
            {
                var rfq = await _rfqService.GetByIdAsync(id);
                if (rfq == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "RFQ not found",
                        Code = "RFQ_NOT_FOUND"
                    });
                }

                // Parse status
                if (!Enum.TryParse<RfqStatus>(request.Status, true, out var newStatus))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "Invalid status. Valid values: Draft, Published, Closed, Cancelled",
                        Code = "INVALID_STATUS"
                    });
                }

                // Update based on status
                try
                {
                    switch (newStatus)
                    {
                        case RfqStatus.Published:
                            await _rfqService.PublishAsync(id);
                            break;
                        case RfqStatus.Closed:
                            await _rfqService.CloseAsync(id);
                            break;
                        case RfqStatus.Cancelled:
                            await _rfqService.CancelAsync(id);
                            break;
                        default:
                            return BadRequest(new ApiResponse<object>
                            {
                                Success = false,
                                Error = "Cannot update to this status via API",
                                Code = "STATUS_NOT_ALLOWED"
                            });
                    }
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Error = ex.Message,
                        Code = "STATUS_UPDATE_FAILED"
                    });
                }

                _logger.LogInformation("RFQ {Id} status updated to {Status} via API", id, newStatus);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new
                    {
                        id = id,
                        newStatus = newStatus.ToString(),
                        message = "Status updated successfully"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating RFQ {Id} status", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = "An error occurred while updating status",
                    Code = "INTERNAL_ERROR"
                });
            }
        }
    }

    #region API DTOs

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
        public string? Code { get; set; }
        public List<string>? Errors { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class RfqListApiDto
    {
        public int Id { get; set; }
        public string RfqNumber { get; set; } = "";
        public string Title { get; set; } = "";
        public string? CompanyName { get; set; }
        public string? SectorName { get; set; }
        public string Status { get; set; } = "";
        public DateTime EndDate { get; set; }
        public int ItemCount { get; set; }
        public int QuotationCount { get; set; }
    }

    public class RfqCreateApiRequest
    {
        public string Title { get; set; } = "";
        public int SectorId { get; set; }
        public int CompanyId { get; set; }
        public RfqVisibility Visibility { get; set; } = RfqVisibility.AllSector;
        public DateTime? StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Currency { get; set; }
        public string? ExternalReference { get; set; }
        public List<RfqItemApiRequest>? Items { get; set; }
    }

    public class RfqItemApiRequest
    {
        public int? MaterialId { get; set; }
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public string? TechnicalSpecs { get; set; }
    }

    public class RfqCreateResult
    {
        public int Id { get; set; }
        public string RfqNumber { get; set; } = "";
        public string Status { get; set; } = "";
        public string Message { get; set; } = "";
    }

    public class StatusUpdateRequest
    {
        public string Status { get; set; } = "";
    }

    #endregion
}
