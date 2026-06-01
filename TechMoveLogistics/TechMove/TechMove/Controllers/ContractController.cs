using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMove.Models;
using TechMove.Services;
using TechMove.ViewModel;
  

namespace TechMove.Controllers  
{
    public class ContractController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<ContractController> _logger;

        public ContractController(
            IApiService apiService,
            ILogger<ContractController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchStatus, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                // Get contracts from API with filters
                var contracts = await _apiService.GetContractsAsync(startDate, endDate, searchStatus);

                // Prepare view data for filters
                ViewBag.StatusList = Enum.GetValues(typeof(ContractStatus))
                    .Cast<ContractStatus>()
                    .Select(s => new SelectListItem { Value = s.ToString(), Text = s.ToString() });
                ViewBag.SelectedStatus = searchStatus;
                ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

                return View(contracts ?? new List<Contract>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contracts index");
                TempData["Error"] = "Failed to load contracts. Please ensure the API is running.";
                return View(new List<Contract>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var contract = await _apiService.GetContractByIdAsync(id);
                if (contract == null)
                    return NotFound();

                return View(contract);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contract details for ID {ContractId}", id);
                TempData["Error"] = "Failed to load contract details.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var clients = await _apiService.GetClientsAsync();
                ViewBag.Clients = new SelectList(clients, "ClientId", "Name");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create contract form");
                TempData["Error"] = "Failed to load clients. Please ensure the API is running.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Create contract via API
                    var contract = new Contract
                    {
                        ClientId = model.ClientId,
                        ContractNumber = model.ContractNumber,
                        ServiceLead = model.ServiceLead,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        Status = model.Status,
                        TermsAndConditions = model.TermsAndConditions
                    };

                    // Handle file upload separately (if needed)
                    if (model.SignedAgreement != null && model.SignedAgreement.Length > 0)
                    {
                        var filePath = await _apiService.UploadContractFileAsync(model.SignedAgreement, 0);
                        contract.SignedAgreementPath = filePath;
                    }

                    var createdContract = await _apiService.CreateContractAsync(contract);

                    if (createdContract != null)
                    {
                        _logger.LogInformation("Contract created: {ContractNumber}", createdContract.ContractNumber);
                        TempData["Success"] = "Contract created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Error"] = "Failed to create contract.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating contract");
                    ModelState.AddModelError("", $"Error creating contract: {ex.Message}");
                }
            }

            // Reload clients for the view
            var clients = await _apiService.GetClientsAsync();
            ViewBag.Clients = new SelectList(clients, "ClientId", "Name", model.ClientId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var contract = await _apiService.GetContractByIdAsync(id);
                if (contract == null)
                    return NotFound();

                var clients = await _apiService.GetClientsAsync();

                var model = new ContractViewModel
                {
                    ContractId = contract.ContractId,
                    ClientId = contract.ClientId,
                    ContractNumber = contract.ContractNumber,
                    ServiceLead = contract.ServiceLead,
                    StartDate = contract.StartDate,
                    EndDate = contract.EndDate,
                    Status = contract.Status,
                    TermsAndConditions = contract.TermsAndConditions,
                    ExistingFilePath = contract.SignedAgreementPath
                };

                ViewBag.Clients = new SelectList(clients, "ClientId", "Name", contract.ClientId);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for contract {ContractId}", id);
                TempData["Error"] = "Failed to load contract for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContractViewModel model)
        {
            if (id != model.ContractId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var contract = new Contract
                    {
                        ContractId = model.ContractId,
                        ClientId = model.ClientId,
                        ContractNumber = model.ContractNumber,
                        ServiceLead = model.ServiceLead,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        Status = model.Status,
                        TermsAndConditions = model.TermsAndConditions,
                        SignedAgreementPath = model.ExistingFilePath,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // Handle new file upload
                    if (model.SignedAgreement != null && model.SignedAgreement.Length > 0)
                    {
                        var filePath = await _apiService.UploadContractFileAsync(model.SignedAgreement, contract.ContractId);
                        contract.SignedAgreementPath = filePath;
                    }

                    var updated = await _apiService.UpdateContractAsync(contract);

                    if (updated)
                    {
                        _logger.LogInformation("Contract updated: {ContractNumber}", contract.ContractNumber);
                        TempData["Success"] = "Contract updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Error"] = "Failed to update contract.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating contract {ContractId}", id);
                    ModelState.AddModelError("", $"Error updating contract: {ex.Message}");
                }
            }

            var clients = await _apiService.GetClientsAsync();
            ViewBag.Clients = new SelectList(clients, "ClientId", "Name", model.ClientId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var contract = await _apiService.GetContractByIdAsync(id);
                if (contract == null)
                    return NotFound();

                // Check if there are service requests
                var serviceRequests = await _apiService.GetServiceRequestsByContractAsync(id);
                if (serviceRequests != null && serviceRequests.Any())
                {
                    TempData["Error"] = "Cannot delete contract with existing service requests.";
                    return RedirectToAction(nameof(Index));
                }

                return View(contract);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete confirmation for contract {ContractId}", id);
                TempData["Error"] = "Failed to load contract for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var deleted = await _apiService.DeleteContractAsync(id);
                if (deleted)
                {
                    _logger.LogInformation("Contract deleted: {ContractId}", id);
                    TempData["Success"] = "Contract deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete contract.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contract {ContractId}", id);
                TempData["Error"] = $"Error deleting contract: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DownloadAgreement(int id)
        {
            try
            {
                var contract = await _apiService.GetContractByIdAsync(id);
                if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
                    return NotFound();

                var fileBytes = await _apiService.DownloadContractFileAsync(contract.SignedAgreementPath);
                if (fileBytes == null)
                {
                    TempData["Error"] = "File not found.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var fileName = $"Contract_{contract.ContractNumber}_Agreement.pdf";
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading agreement for contract {ContractId}", id);
                TempData["Error"] = "Could not download the file.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var updated = await _apiService.UpdateContractStatusAsync(id, status);
                if (updated)
                {
                    TempData["Success"] = "Contract status updated successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to update contract status.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for contract {ContractId}", id);
                TempData["Error"] = $"Error updating status: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}