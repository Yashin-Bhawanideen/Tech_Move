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
    
    public class ClientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(ApplicationDbContext context, ILogger<ClientsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients()
        {
            try
            {
                var clients = await _context.Clients
                    .Include(c => c.Contracts)
                    .OrderBy(c => c.Name)
                    .Select(c => new ClientDto
                    {
                        ClientId = c.ClientId,
                        Name = c.Name,
                        Email = c.Email,
                        Phone = c.Phone,
                        Address = c.Address,
                        Region = c.Region,
                        CreatedAt = c.CreatedAt,
                        ContractsCount = c.Contracts.Count
                    })
                    .ToListAsync();

                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clients");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClientDto>> GetClient(int id)
        {
            try
            {
                var client = await _context.Clients
                    .Include(c => c.Contracts)
                    .FirstOrDefaultAsync(c => c.ClientId == id);

                if (client == null)
                    return NotFound(new { message = $"Client with ID {id} not found" });

                return Ok(new ClientDto
                {
                    ClientId = client.ClientId,
                    Name = client.Name,
                    Email = client.Email,
                    Phone = client.Phone,
                    Address = client.Address,
                    Region = client.Region,
                    CreatedAt = client.CreatedAt,
                    ContractsCount = client.Contracts.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client {ClientId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ClientDto>> CreateClient(CreateClientDto createDto)
        {
            try
            {
                // Log received data
                _logger.LogInformation("Received client creation request: {ClientName}", createDto.Name);

                // Validate client exists
                if (createDto == null)
                {
                    return BadRequest(new { message = "Client data is null" });
                }

                // Check for duplicate email
                if (await _context.Clients.AnyAsync(c => c.Email == createDto.Email))
                {
                    return BadRequest(new { message = "A client with this email already exists" });
                }

                var client = new Client
                {
                    Name = createDto.Name,
                    Email = createDto.Email,
                    Phone = createDto.Phone,
                    Address = createDto.Address,
                    Region = createDto.Region,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Client created successfully with ID: {ClientId}", client.ClientId);

                return Ok(new ClientDto
                {
                    ClientId = client.ClientId,
                    Name = client.Name,
                    Email = client.Email,
                    Phone = client.Phone,
                    Address = client.Address,
                    Region = client.Region,
                    CreatedAt = client.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, UpdateClientDto updateDto)
        {
            try
            {
                var client = await _context.Clients.FindAsync(id);
                if (client == null)
                    return NotFound(new { message = $"Client with ID {id} not found" });

                // Check for duplicate email (excluding current client)
                if (await _context.Clients.AnyAsync(c => c.Email == updateDto.Email && c.ClientId != id))
                    return BadRequest(new { message = "A client with this email already exists" });

                client.Name = updateDto.Name;
                client.Email = updateDto.Email;
                client.Phone = updateDto.Phone;
                client.Address = updateDto.Address;
                client.Region = updateDto.Region;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Client updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client {ClientId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            try
            {
                var client = await _context.Clients
                    .Include(c => c.Contracts)
                    .FirstOrDefaultAsync(c => c.ClientId == id);

                if (client == null)
                    return NotFound(new { message = $"Client with ID {id} not found" });

                if (client.Contracts.Any())
                    return BadRequest(new { message = "Cannot delete client with existing contracts" });

                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Client deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client {ClientId}", id);
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }
    }
}