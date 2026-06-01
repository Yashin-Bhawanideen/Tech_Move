using Microsoft.AspNetCore.Mvc;
using TechMove.Models;
using TechMove.Services;

namespace TechMove.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<ServiceRequestController> _logger;

        public ServiceRequestController(IApiService apiService, ILogger<ServiceRequestController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string status, string contractNumber)
        {
            try
            {
                var requests = await _apiService.GetServiceRequestsAsync(status, contractNumber);
                return View(requests ?? new List<ServiceRequest>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service requests");
                TempData["Error"] = "Failed to load service requests. Please ensure the API is running.";
                return View(new List<ServiceRequest>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var request = await _apiService.GetServiceRequestByIdAsync(id);
                if (request == null)
                    return NotFound();
                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading service request details for ID {RequestId}", id);
                TempData["Error"] = "Failed to load service request details.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var activeContracts = await _apiService.GetActiveContractsAsync();
                ViewBag.Contracts = activeContracts ?? new List<Contract>();
                ViewBag.CurrentRate = await _apiService.GetExchangeRateAsync();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create service request form");
                TempData["Error"] = "Failed to load form. Please ensure the API is running.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContractId,RequestTitle,Description,AmountUSD")] ServiceRequest serviceRequest)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var created = await _apiService.CreateServiceRequestAsync(serviceRequest);
                    if (created != null)
                    {
                        _logger.LogInformation("Service request created for contract {ContractId}", serviceRequest.ContractId);
                        TempData["Success"] = "Service request created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Error"] = "Failed to create service request.";
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "API error creating service request");
                    TempData["Error"] = "Cannot connect to the API. Please ensure it's running.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating service request");
                    ModelState.AddModelError("", $"Error creating request: {ex.Message}");
                }
            }

            // Reload data for form
            var activeContracts = await _apiService.GetActiveContractsAsync();
            ViewBag.Contracts = activeContracts ?? new List<Contract>();
            ViewBag.CurrentRate = await _apiService.GetExchangeRateAsync();
            return View(serviceRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var updated = await _apiService.UpdateServiceRequestStatusAsync(id, status);
                if (updated)
                {
                    _logger.LogInformation("Service request {RequestId} status updated to {Status}", id, status);
                    TempData["Success"] = "Status updated successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to update status.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for request {RequestId}", id);
                TempData["Error"] = $"Error updating status: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<JsonResult> GetExchangeRate()
        {
            try
            {
                var rate = await _apiService.GetExchangeRateAsync();
                return Json(new { success = true, rate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exchange rate");
                return Json(new { success = false, message = "Failed to get exchange rate" });
            }
        }

        [HttpGet]
        public async Task<JsonResult> ConvertCurrency(decimal usdAmount)
        {
            try
            {
                var zarAmount = await _apiService.ConvertCurrencyAsync(usdAmount);
                var rate = await _apiService.GetExchangeRateAsync();
                return Json(new { success = true, usd = usdAmount, zar = zarAmount, rate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting currency");
                return Json(new { success = false, message = "Failed to convert currency" });
            }
        }
    }
}