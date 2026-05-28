using System.Text;
using System.Text.Json;
using TechMove.Models;

namespace TechMove.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiService> _logger;
        private string _token;

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private async Task SetAuthorizationHeader()
        {
            if (string.IsNullOrEmpty(_token))
            {
                var token = _httpContextAccessor.HttpContext?.Session?.GetString("AuthToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _token = token;
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                }
            }
        }

        // Authentication Methods
        public async Task<string> LoginAsync(string username, string password)
        {
            try
            {
                var loginData = new { username, password };
                var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var tokenObj = JsonSerializer.Deserialize<Dictionary<string, string>>(result);
                    _token = tokenObj?["token"];

                    if (!string.IsNullOrEmpty(_token))
                    {
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                        _httpContextAccessor.HttpContext?.Session?.SetString("AuthToken", _token);

                        _logger.LogInformation("User {Username} logged in successfully", username);
                        return _token;
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Login failed for user {Username}: {Error}", username, error);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", username);
                return null;
            }
        }

        public async Task<string?> GetTokenAsync(string username, string password)
        {
            return await LoginAsync(username, password);
        }

        public async Task LogoutAsync()
        {
            _token = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpContextAccessor.HttpContext?.Session?.Remove("AuthToken");
            _logger.LogInformation("User logged out successfully");
        }

        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(_token) ||
                   !string.IsNullOrEmpty(_httpContextAccessor.HttpContext?.Session?.GetString("AuthToken"));
        }

        // Client Methods
        public async Task<List<Client>> GetClientsAsync()
        {
            await SetAuthorizationHeader();
            try
            {
                var response = await _httpClient.GetAsync("api/clients");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Client>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                           ?? new List<Client>();
                }
                return new List<Client>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clients");
                return new List<Client>();
            }
        }

        public async Task<Client> GetClientByIdAsync(int id)
        {
            await SetAuthorizationHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/clients/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Client>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client {ClientId}", id);
                return null;
            }
        }

        public async Task<Client> CreateClientAsync(Client client)
        {
            await SetAuthorizationHeader();
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(client), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/clients", content);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Client>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to create client: {Error}", error);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                return null;
            }
        }

        public async Task<bool> UpdateClientAsync(Client client)
        {
            await SetAuthorizationHeader();
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(client), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/clients/{client.ClientId}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client {ClientId}", client.ClientId);
                return false;
            }
        }

        public async Task<bool> DeleteClientAsync(int id)
        {
            await SetAuthorizationHeader();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/clients/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client {ClientId}", id);
                return false;
            }
        }

        // Contract Methods
        public async Task<List<Contract>> GetContractsAsync(DateTime? startDate = null, DateTime? endDate = null, string status = null)
        {
            await SetAuthorizationHeader();
            try
            {
                var query = new List<string>();
                if (startDate.HasValue) query.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue) query.Add($"endDate={endDate.Value:yyyy-MM-dd}");
                if (!string.IsNullOrEmpty(status)) query.Add($"status={status}");

                var url = "api/contracts";
                if (query.Any()) url += "?" + string.Join("&", query);

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Contract>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                           ?? new List<Contract>();
                }
                return new List<Contract>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contracts");
                return new List<Contract>();
            }
        }

        public async Task<Contract> GetContractByIdAsync(int id)
        {
            await SetAuthorizationHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/contracts/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Contract>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contract {ContractId}", id);
                return null;
            }
        }

        public async Task<Contract> CreateContractAsync(Contract contract)
        {
            await SetAuthorizationHeader();
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(contract), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/contracts", content);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Contract>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to create contract: {Error}", error);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contract");
                return null;
            }
        }

        public async Task<bool> UpdateContractAsync(Contract contract)
        {
            await SetAuthorizationHeader();
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(contract), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/contracts/{contract.ContractId}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contract {ContractId}", contract.ContractId);
                return false;
            }
        }

        public async Task<bool> DeleteContractAsync(int id)
        {
            await SetAuthorizationHeader();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/contracts/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contract {ContractId}", id);
                return false;
            }
        }

        public async Task<bool> UpdateContractStatusAsync(int id, string status)
        {
            await SetAuthorizationHeader();
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(new { status }), Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"api/contracts/{id}/status", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contract status {ContractId}", id);
                return false;
            }
        }

        public async Task<List<Contract>> GetActiveContractsAsync()
        {
            await SetAuthorizationHeader();
            try
            {
                var response = await _httpClient.GetAsync("api/contracts/active");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Contract>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                           ?? new List<Contract>();
                }
                return new List<Contract>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active contracts");
                return new List<Contract>();
            }
        }

        // Service Request Methods
        public async Task<List<ServiceRequest>> GetServiceRequestsAsync(string status = null, string contractNumber = null)
        {
            await SetAuthorizationHeader();
            try
            {
                var query = new List<string>();
                if (!string.IsNullOrEmpty(status)) query.Add($"status={status}");
                if (!string.IsNullOrEmpty(contractNumber)) query.Add($"contractNumber={contractNumber}");

                var url = "api/servicerequests";
                if (query.Any()) url += "?" + string.Join("&", query);

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<ServiceRequest>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                           ?? new List<ServiceRequest>();
                }
                return new List<ServiceRequest>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service requests");
                return new List<ServiceRequest>();
            }
        }

        public async Task<ServiceRequest> GetServiceRequestByIdAsync(int id)
        {
            await SetAuthorizationHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/servicerequests/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ServiceRequest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service request {RequestId}", id);
                return null;
            }
        }

        public async Task<ServiceRequest> CreateServiceRequestAsync(ServiceRequest request)
        {
            await SetAuthorizationHeader();
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/servicerequests", content);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ServiceRequest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to create service request: {Error}", error);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service request");
                return null;
            }
        }

        public async Task<bool> UpdateServiceRequestStatusAsync(int id, string status)
        {
            await SetAuthorizationHeader();
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(new { status }), Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"api/servicerequests/{id}/status", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service request status {RequestId}", id);
                return false;
            }
        }

        public async Task<List<ServiceRequest>> GetServiceRequestsByContractAsync(int contractId)
        {
            await SetAuthorizationHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/servicerequests/by-contract/{contractId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<ServiceRequest>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                           ?? new List<ServiceRequest>();
                }
                return new List<ServiceRequest>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service requests for contract {ContractId}", contractId);
                return new List<ServiceRequest>();
            }
        }

        // File Handling Methods
        public async Task<string> UploadContractFileAsync(IFormFile file, int contractId)
        {
            await SetAuthorizationHeader();
            try
            {
                using var content = new MultipartFormDataContent();
                using var stream = file.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "file", file.FileName);
                content.Add(new StringContent(contractId.ToString()), "contractId");

                var response = await _httpClient.PostAsync("api/contracts/upload", content);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    return result?["filePath"];
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to upload file: {Error}", error);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file for contract {ContractId}", contractId);
                return null;
            }
        }

        public async Task<byte[]> DownloadContractFileAsync(string filePath)
        {
            await SetAuthorizationHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/contracts/download?path={Uri.EscapeDataString(filePath)}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file from {FilePath}", filePath);
                return null;
            }
        }

        // Currency Methods
        public async Task<decimal> GetExchangeRateAsync()
        {
            await SetAuthorizationHeader();
            try
            {
                var response = await _httpClient.GetAsync("api/servicerequests/exchangerate");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (result != null && result.ContainsKey("rate"))
                    {
                        return Convert.ToDecimal(result["rate"]);
                    }
                }
                return 18.50m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exchange rate");
                return 18.50m;
            }
        }

        public async Task<decimal> ConvertCurrencyAsync(decimal usdAmount)
        {
            await SetAuthorizationHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/servicerequests/convert?usdAmount={usdAmount}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (result != null && result.ContainsKey("zar"))
                    {
                        return Convert.ToDecimal(result["zar"]);
                    }
                }
                return usdAmount * 18.50m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting currency for amount {UsdAmount}", usdAmount);
                return usdAmount * 18.50m;
            }
        }
    }
}