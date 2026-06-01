using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove.API.Data;
using TechMove.API.DTOs;
using TechMove.API.Models;

using TechMove.API.Services;

namespace TechMove.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class ServiceRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyExchangeService _currencyService;
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(
            ApplicationDbContext context,
            ICurrencyExchangeService currencyService,
            ILogger<ServiceRequestsController> logger)
        {
            _context = context;
            _currencyService = currencyService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceRequestDto>>> GetServiceRequests(
            [FromQuery] string? status,
            [FromQuery] string? contractNumber)
        {
            try
            {
                var query = _context.ServiceRequests
                    .Include(sr => sr.Contract)
                    .ThenInclude(c => c.Client)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status) && Enum.TryParse<RequestStatus>(status, true, out var statusFilter))
                    query = query.Where(sr => sr.Status == statusFilter);

                if (!string.IsNullOrEmpty(contractNumber))
                    query = query.Where(sr => sr.Contract.ContractNumber.Contains(contractNumber));

                var requests = await query
                    .OrderByDescending(sr => sr.CreatedAt)
                    .Select(sr => new ServiceRequestDto
                    {
                        ServiceRequestId = sr.ServiceRequestId,
                        ContractId = sr.ContractId,
                        ContractNumber = sr.Contract.ContractNumber,
                        ClientName = sr.Contract.Client.Name,
                        RequestTitle = sr.RequestTitle,
                        Description = sr.Description,
                        AmountUSD = sr.AmountUSD,
                        AmountZAR = sr.AmountZAR,
                        Status = sr.Status.ToString(),
                        ExchangeRateUsed = sr.ExchangeRateUsed,
                        CreatedAt = sr.CreatedAt
                    })
                    .ToListAsync();

                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service requests");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceRequestDto>> GetServiceRequest(int id)
        {
            try
            {
                var request = await _context.ServiceRequests
                    .Include(sr => sr.Contract)
                    .ThenInclude(c => c.Client)
                    .FirstOrDefaultAsync(sr => sr.ServiceRequestId == id);

                if (request == null)
                    return NotFound(new { message = $"Service request with ID {id} not found" });

                return Ok(new ServiceRequestDto
                {
                    ServiceRequestId = request.ServiceRequestId,
                    ContractId = request.ContractId,
                    ContractNumber = request.Contract.ContractNumber,
                    ClientName = request.Contract.Client.Name,
                    RequestTitle = request.RequestTitle,
                    Description = request.Description,
                    AmountUSD = request.AmountUSD,
                    AmountZAR = request.AmountZAR,
                    Status = request.Status.ToString(),
                    ExchangeRateUsed = request.ExchangeRateUsed,
                    CreatedAt = request.CreatedAt,
                    UpdatedAt = request.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service request {RequestId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ServiceRequestDto>> CreateServiceRequest(CreateServiceRequestDto createDto)
        {
            try
            {
                // Validate contract exists and is active
                var contract = await _context.Contracts
                    .Include(c => c.Client)
                    .FirstOrDefaultAsync(c => c.ContractId == createDto.ContractId);

                if (contract == null)
                    return BadRequest(new { message = "Contract not found" });

                var today = DateTime.UtcNow.Date;
                if (contract.Status != ContractStatus.Active || contract.StartDate > today || contract.EndDate < today)
                    return BadRequest(new { message = "Service requests can only be created for active contracts" });

                // Get exchange rate and convert currency
                var exchangeRate = await _currencyService.GetUSDtoZARRateAsync();
                var amountZAR = await _currencyService.ConvertUSDtoZARAsync(createDto.AmountUSD);

                var serviceRequest = new ServiceRequest
                {
                    ContractId = createDto.ContractId,
                    RequestTitle = createDto.RequestTitle,
                    Description = createDto.Description,
                    AmountUSD = createDto.AmountUSD,
                    AmountZAR = amountZAR,
                    ExchangeRateUsed = exchangeRate,
                    Status = RequestStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ServiceRequests.Add(serviceRequest);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetServiceRequest), new { id = serviceRequest.ServiceRequestId }, serviceRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service request");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateRequestStatus(int id, [FromBody] UpdateRequestStatusDto statusDto)
        {
            try
            {
                var request = await _context.ServiceRequests.FindAsync(id);
                if (request == null)
                    return NotFound(new { message = $"Service request with ID {id} not found" });

                if (!Enum.TryParse<RequestStatus>(statusDto.Status, true, out var newStatus))
                    return BadRequest(new { message = "Invalid status value" });

                request.Status = newStatus;
                request.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Status updated successfully", status = newStatus.ToString() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service request status");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        [HttpGet("exchangerate")]
        public async Task<ActionResult<object>> GetExchangeRate()
        {
            try
            {
                var rate = await _currencyService.GetUSDtoZARRateAsync();
                return Ok(new { success = true, rate, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exchange rate");
                return Ok(new { success = false, message = "Failed to get exchange rate", rate = 18.50m });
            }
        }

        [HttpGet("convert")]
        public async Task<ActionResult<object>> ConvertCurrency([FromQuery] decimal usdAmount)
        {
            try
            {
                var zarAmount = await _currencyService.ConvertUSDtoZARAsync(usdAmount);
                var rate = await _currencyService.GetUSDtoZARRateAsync();
                return Ok(new { success = true, usd = usdAmount, zar = zarAmount, rate, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting currency");
                return Ok(new { success = false, message = "Failed to convert currency", usd = usdAmount, zar = usdAmount * 18.50m });
            }
        }
    }
}