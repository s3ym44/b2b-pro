using Microsoft.AspNetCore.Mvc;
using B2BProcurement.Attributes;
using B2BProcurement.Business.Interfaces;
using B2BProcurement.Business.DTOs.Quotation;
using B2BProcurement.Core.Enums;

namespace B2BProcurement.Controllers.Api
{
    /// <summary>
    /// Quotation API Controller for external integrations.
    /// </summary>
    [Route("api/quotation")]
    [ApiController]
    [ApiKeyAuth]
    [RateLimit(MaxRequests = 100, WindowSeconds = 60)]
    [Produces("application/json")]
    public class QuotationApiController : ControllerBase
    {
        private readonly IQuotationService _quotationService;
        private readonly IRfqService _rfqService;
        private readonly ILogger<QuotationApiController> _logger;

        public QuotationApiController(
            IQuotationService quotationService,
            IRfqService rfqService,
            ILogger<QuotationApiController> logger)
        {
            _quotationService = quotationService;
            _rfqService = rfqService;
            _logger = logger;
        }

        /// <summary>
        /// Get quotations for a specific RFQ.
        /// </summary>
        /// <param name="rfqId">RFQ ID</param>
        /// <param name="status">Filter by status</param>
        [HttpGet("by-rfq/{rfqId}")]
        [ProducesResponseType(typeof(ApiResponse<List<QuotationListApiDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetByRfq(int rfqId, [FromQuery] QuotationStatus? status = null)
        {
            try
            {
                var rfq = await _rfqService.GetByIdAsync(rfqId);
                if (rfq == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "RFQ not found",
                        Code = "RFQ_NOT_FOUND"
                    });
                }

                var quotations = await _quotationService.GetByRfqAsync(rfqId);

                if (status.HasValue)
                {
                    quotations = quotations.Where(q => q.Status == status.Value);
                }

                var result = quotations.Select(q => new QuotationListApiDto
                {
                    Id = q.Id,
                    QuotationNumber = q.QuotationNumber,
                    RfqNumber = q.RfqNumber,
                    SupplierCompanyName = q.SupplierCompanyName ?? "",
                    Status = q.Status.ToString(),
                    TotalAmount = q.TotalAmount,
                    ValidUntil = q.ValidUntil,
                    ItemCount = q.ItemCount
                }).ToList();

                return Ok(new ApiResponse<List<QuotationListApiDto>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quotations for RFQ {RfqId}", rfqId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = "An error occurred while retrieving quotations",
                    Code = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Get quotation details by ID.
        /// </summary>
        /// <param name="id">Quotation ID</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<QuotationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var quotation = await _quotationService.GetByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "Quotation not found",
                        Code = "QUOTATION_NOT_FOUND"
                    });
                }

                return Ok(new ApiResponse<QuotationDto>
                {
                    Success = true,
                    Data = quotation
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quotation {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = "An error occurred while retrieving quotation",
                    Code = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Create a new quotation for an RFQ.
        /// </summary>
        /// <param name="request">Quotation creation request</param>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<QuotationCreateResult>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Create([FromBody] QuotationCreateApiRequest request)
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

                // Check if RFQ exists and is open
                var rfq = await _rfqService.GetByIdAsync(request.RfqId);
                if (rfq == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "RFQ not found",
                        Code = "RFQ_NOT_FOUND"
                    });
                }

                if (rfq.Status != RfqStatus.Published)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "RFQ is not open for quotations",
                        Code = "RFQ_NOT_OPEN"
                    });
                }

                // Map to QuotationCreateDto
                var createDto = new QuotationCreateDto
                {
                    RfqId = request.RfqId,
                    ValidUntil = request.ValidUntil,
                    Items = request.Items?.Select(i => new QuotationItemCreateDto
                    {
                        RfqItemId = i.RfqItemId,
                        UnitPrice = i.UnitPrice,
                        OfferedQuantity = i.OfferedQuantity,
                        DeliveryDate = i.DeliveryDate
                    }).ToList() ?? new List<QuotationItemCreateDto>()
                };

                var result = await _quotationService.CreateAsync(request.SupplierCompanyId, createDto);

                _logger.LogInformation("Quotation created via API: {QuotationNumber}", result.QuotationNumber);

                return CreatedAtAction(nameof(GetById), new { id = result.Id }, new ApiResponse<QuotationCreateResult>
                {
                    Success = true,
                    Data = new QuotationCreateResult
                    {
                        Id = result.Id,
                        QuotationNumber = result.QuotationNumber,
                        Status = result.Status.ToString(),
                        TotalAmount = result.TotalAmount,
                        Message = "Quotation created successfully"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quotation");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = "An error occurred while creating quotation",
                    Code = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Get comparison data for quotations of an RFQ.
        /// </summary>
        /// <param name="id">RFQ ID</param>
        [HttpGet("{id}/comparison")]
        [ProducesResponseType(typeof(ApiResponse<QuotationComparisonApiDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetComparison(int id)
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

                var comparison = await _quotationService.GetComparisonMatrixAsync(id);
                if (comparison == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Error = "No comparison data available",
                        Code = "NO_COMPARISON_DATA"
                    });
                }

                var result = new QuotationComparisonApiDto
                {
                    RfqId = comparison.RfqId,
                    RfqNumber = comparison.RfqNumber ?? "",
                    RfqTitle = comparison.RfqTitle ?? "",
                    Items = comparison.Items.Select(i => new ComparisonItemApiDto
                    {
                        RfqItemId = i.RfqItemId,
                        Description = i.Description,
                        Quantity = i.RequestedQuantity,
                        Unit = i.Unit,
                        Prices = i.Prices.Select(p => new ComparisonPriceApiDto
                        {
                            SupplierCompanyId = p.SupplierCompanyId,
                            SupplierName = p.SupplierName ?? "",
                            UnitPrice = p.UnitPrice,
                            TotalPrice = p.TotalPrice,
                            DeliveryDate = p.DeliveryDate,
                            IsLowestPrice = p.IsLowestPrice
                        }).ToList()
                    }).ToList(),
                    Suppliers = comparison.Suppliers.Select(s => new ComparisonSupplierApiDto
                    {
                        CompanyId = s.CompanyId,
                        CompanyName = s.CompanyName,
                        QuotationNumber = s.QuotationNumber,
                        TotalAmount = s.TotalAmount
                    }).ToList()
                };

                return Ok(new ApiResponse<QuotationComparisonApiDto>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comparison for RFQ {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = "An error occurred while retrieving comparison",
                    Code = "INTERNAL_ERROR"
                });
            }
        }
    }

    #region Quotation API DTOs

    public class QuotationListApiDto
    {
        public int Id { get; set; }
        public string QuotationNumber { get; set; } = "";
        public string? RfqNumber { get; set; }
        public string SupplierCompanyName { get; set; } = "";
        public string Status { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int ItemCount { get; set; }
    }

    public class QuotationCreateApiRequest
    {
        public int RfqId { get; set; }
        public int SupplierCompanyId { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string? ExternalReference { get; set; }
        public List<QuotationItemApiRequest>? Items { get; set; }
    }

    public class QuotationItemApiRequest
    {
        public int RfqItemId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OfferedQuantity { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }

    public class QuotationCreateResult
    {
        public int Id { get; set; }
        public string QuotationNumber { get; set; } = "";
        public string Status { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public string Message { get; set; } = "";
    }

    public class QuotationComparisonApiDto
    {
        public int RfqId { get; set; }
        public string RfqNumber { get; set; } = "";
        public string RfqTitle { get; set; } = "";
        public List<ComparisonItemApiDto> Items { get; set; } = new();
        public List<ComparisonSupplierApiDto> Suppliers { get; set; } = new();
    }

    public class ComparisonItemApiDto
    {
        public int RfqItemId { get; set; }
        public string Description { get; set; } = "";
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "";
        public List<ComparisonPriceApiDto> Prices { get; set; } = new();
    }

    public class ComparisonSupplierApiDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = "";
        public string QuotationNumber { get; set; } = "";
        public decimal TotalAmount { get; set; }
    }

    public class ComparisonPriceApiDto
    {
        public int SupplierCompanyId { get; set; }
        public string SupplierName { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public bool IsLowestPrice { get; set; }
    }

    #endregion
}
