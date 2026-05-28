using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Models;

namespace TechMove.Repositories
{
    public class ServiceRequestRepository : Repository<ServiceRequest>, IServiceRequestRepository
    {
        public ServiceRequestRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ServiceRequest>> GetServiceRequestsWithDetailsAsync()
        {
            return await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .ThenInclude(c => c.Client)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync();
        }

        public async Task<ServiceRequest?> GetServiceRequestWithDetailsAsync(int id)
        {
            return await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(sr => sr.ServiceRequestId == id);
        }

        public async Task<IEnumerable<ServiceRequest>> GetServiceRequestsByContractAsync(int contractId)
        {
            return await _context.ServiceRequests
                .Where(sr => sr.ContractId == contractId)
                .Include(sr => sr.Contract)
                .ThenInclude(c => c.Client)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync();
        }

       
    }
}
