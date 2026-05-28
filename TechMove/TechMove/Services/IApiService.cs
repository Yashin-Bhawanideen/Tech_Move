
using TechMove.Models;

namespace TechMove.Services
{
    public interface IApiService
    {
        // Authentication
        Task<string> LoginAsync(string username, string password);
        Task LogoutAsync();
        bool IsAuthenticated();

        // Client methods
        Task<List<Client>> GetClientsAsync();
        Task<Client> GetClientByIdAsync(int id);
        Task<Client> CreateClientAsync(Client client);
        Task<bool> UpdateClientAsync(Client client);
        Task<bool> DeleteClientAsync(int id);

        // Contract methods
        Task<List<Contract>> GetContractsAsync(DateTime? startDate = null, DateTime? endDate = null, string status = null);
        Task<Contract> GetContractByIdAsync(int id);
        Task<Contract> CreateContractAsync(Contract contract);
        Task<bool> UpdateContractAsync(Contract contract);
        Task<bool> DeleteContractAsync(int id);
        Task<bool> UpdateContractStatusAsync(int id, string status);
        Task<List<Contract>> GetActiveContractsAsync();

        // Service Request methods
        Task<List<ServiceRequest>> GetServiceRequestsAsync(string status = null, string contractNumber = null);
        Task<ServiceRequest> GetServiceRequestByIdAsync(int id);
        Task<ServiceRequest> CreateServiceRequestAsync(ServiceRequest request);
        Task<bool> UpdateServiceRequestStatusAsync(int id, string status);
        Task<List<ServiceRequest>> GetServiceRequestsByContractAsync(int contractId);

        // File handling
        Task<string> UploadContractFileAsync(IFormFile file, int contractId);
        Task<byte[]> DownloadContractFileAsync(string filePath);

        // Currency
        Task<decimal> GetExchangeRateAsync();
        Task<decimal> ConvertCurrencyAsync(decimal usdAmount);
        Task<string?> GetTokenAsync(string username, string password);
    }
}
