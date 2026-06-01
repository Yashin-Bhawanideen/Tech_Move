using Microsoft.AspNetCore.Mvc;
using TechMove.Models;
using TechMove.Services;

namespace TechMove.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiService _apiService;

        public HomeController(ILogger<HomeController> logger, IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Fetch all data
                var clients = await _apiService.GetClientsAsync();
                var contracts = await _apiService.GetContractsAsync();
                var requests = await _apiService.GetServiceRequestsAsync();

                // Set ViewBag properties - Compare with enum values
                ViewBag.TotalClients = clients?.Count ?? 0;
                ViewBag.ActiveContracts = contracts?.Count(c => c.Status == ContractStatus.Active) ?? 0;
                ViewBag.PendingRequests = requests?.Count(r => r.Status == RequestStatus.Pending) ?? 0;
                ViewBag.TotalRequests = requests?.Count ?? 0;
                ViewBag.ApiConnected = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dashboard data");
                ViewBag.TotalClients = 0;
                ViewBag.ActiveContracts = 0;
                ViewBag.PendingRequests = 0;
                ViewBag.TotalRequests = 0;
                ViewBag.ApiConnected = false;
                ViewBag.Error = "Unable to connect to the API. Please make sure the API is running.";
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}