using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove.API.Data;
using TechMove.API.DTOs;
using TechMove.API.Models;

namespace TechMove.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContractsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(ApplicationDbContext context, ILogger<ContractsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContractDto>>> GetContracts(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? status)
        {
            try
            {
                _logger.LogInformation("Getting contracts with filters");

                var query = _context.Contracts
                    .Include(c => c.Client)
                    .AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(c => c.StartDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(c => c.EndDate <= endDate.Value);

                if (!string.IsNullOrEmpty(status) && Enum.TryParse<ContractStatus>(status, true, out var statusFilter))
                    query = query.Where(c => c.Status == statusFilter);

                var contracts = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new ContractDto
                    {
                        ContractId = c.ContractId,
                        ClientId = c.ClientId,
                        ClientName = c.Client.Name,
                        ContractNumber = c.ContractNumber,
                        ServiceLead = c.ServiceLead,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Status = c.Status.ToString(),
                        SignedAgreementPath = c.SignedAgreementPath,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                return Ok(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contracts");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContractDto>> GetContract(int id)
        {
            try
            {
                var contract = await _context.Contracts
                    .Include(c => c.Client)
                    .Include(c => c.ServiceRequests)
                    .FirstOrDefaultAsync(c => c.ContractId == id);

                if (contract == null)
                    return NotFound(new { message = $"Contract with ID {id} not found" });

                return Ok(new ContractDto
                {
                    ContractId = contract.ContractId,
                    ClientId = contract.ClientId,
                    ClientName = contract.Client.Name,
                    ContractNumber = contract.ContractNumber,
                    ServiceLead = contract.ServiceLead,
                    StartDate = contract.StartDate,
                    EndDate = contract.EndDate,
                    Status = contract.Status.ToString(),
                    SignedAgreementPath = contract.SignedAgreementPath,
                    TermsAndConditions = contract.TermsAndConditions,
                    CreatedAt = contract.CreatedAt,
                    UpdatedAt = contract.UpdatedAt,
                    ServiceRequestsCount = contract.ServiceRequests.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contract {ContractId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ContractDto>> CreateContract(CreateContractDto createDto)
        {
            try
            {
                // Validate client exists
                var client = await _context.Clients.FindAsync(createDto.ClientId);
                if (client == null)
                    return BadRequest(new { message = $"Client with ID {createDto.ClientId} not found" });

                // Validate dates
                if (createDto.StartDate > createDto.EndDate)
                    return BadRequest(new { message = "Start date cannot be after end date" });

                var contract = new Contract
                {
                    ClientId = createDto.ClientId,
                    ContractNumber = createDto.ContractNumber,
                    ServiceLead = createDto.ServiceLead,
                    StartDate = createDto.StartDate,
                    EndDate = createDto.EndDate,
                    Status = Enum.Parse<ContractStatus>(createDto.Status, true),
                    TermsAndConditions = createDto.TermsAndConditions,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetContract), new { id = contract.ContractId }, contract);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contract");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContract(int id, UpdateContractDto updateDto)
        {
            try
            {
                var contract = await _context.Contracts.FindAsync(id);
                if (contract == null)
                    return NotFound(new { message = $"Contract with ID {id} not found" });

                if (updateDto.StartDate > updateDto.EndDate)
                    return BadRequest(new { message = "Start date cannot be after end date" });

                contract.ClientId = updateDto.ClientId;
                contract.ContractNumber = updateDto.ContractNumber;
                contract.ServiceLead = updateDto.ServiceLead;
                contract.StartDate = updateDto.StartDate;
                contract.EndDate = updateDto.EndDate;
                contract.Status = Enum.Parse<ContractStatus>(updateDto.Status, true);
                contract.TermsAndConditions = updateDto.TermsAndConditions;
                contract.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Contract updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contract {ContractId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateContractStatus(int id, [FromBody] UpdateContractStatusDto statusDto)
        {
            try
            {
                var contract = await _context.Contracts.FindAsync(id);
                if (contract == null)
                    return NotFound(new { message = $"Contract with ID {id} not found" });

                if (!Enum.TryParse<ContractStatus>(statusDto.Status, true, out var newStatus))
                    return BadRequest(new { message = "Invalid status value" });

                contract.Status = newStatus;
                contract.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Status updated successfully", status = newStatus.ToString() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contract status");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            try
            {
                var contract = await _context.Contracts
                    .Include(c => c.ServiceRequests)
                    .FirstOrDefaultAsync(c => c.ContractId == id);

                if (contract == null)
                    return NotFound(new { message = $"Contract with ID {id} not found" });

                if (contract.ServiceRequests.Any())
                    return BadRequest(new { message = "Cannot delete contract with existing service requests" });

                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Contract deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contract {ContractId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<ContractDto>>> GetActiveContracts()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var contracts = await _context.Contracts
                    .Include(c => c.Client)
                    .Where(c => c.Status == ContractStatus.Active &&
                               c.StartDate <= today &&
                               c.EndDate >= today)
                    .Select(c => new ContractDto
                    {
                        ContractId = c.ContractId,
                        ClientId = c.ClientId,
                        ClientName = c.Client.Name,
                        ContractNumber = c.ContractNumber,
                        ServiceLead = c.ServiceLead,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Status = c.Status.ToString(),
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                return Ok(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active contracts");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }
    }
}