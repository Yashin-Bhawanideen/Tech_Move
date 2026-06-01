using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Models;

namespace TechMove.Repositories
{
    public class ContractRepository : Repository<Contract>, IContractRepository
    {

        //constructor
        public ContractRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Contract>> GetContractsWithDetailsAsync()
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        }

        public async Task<Contract?> GetContractWithDetailsAsync(int id)
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.ContractId == id);
        }

        public async Task<IEnumerable<Contract>> GetActiveContractsAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active &&
                           c.StartDate <= today &&
                           c.EndDate >= today)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contract>> SearchContractsAsync(DateTime? startDate, DateTime? endDate, ContractStatus? status)
        {
            var query = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<bool> IsContractActiveForServiceAsync(int contractId)
        {
            var contract = await GetByIdAsync(contractId);
            if (contract == null)
                return false;

            var today = DateTime.UtcNow.Date;
            return contract.Status == ContractStatus.Active &&
                   contract.StartDate <= today &&
                   contract.EndDate >= today;
        }

        public async Task<IEnumerable<Client>> GetAvailableClientsAsync()
        {
            return await _context.Clients
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}