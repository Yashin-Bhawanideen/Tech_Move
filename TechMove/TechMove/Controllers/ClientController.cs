using Microsoft.AspNetCore.Mvc;
using TechMove.Models;
using TechMove.Services;

namespace TechMove.Controllers
{
    public class ClientController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IApiService apiService, ILogger<ClientController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var clients = await _apiService.GetClientsAsync();
                return View(clients ?? new List<Client>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading clients index");
                TempData["Error"] = "Failed to load clients. Please ensure the API is running.";
                return View(new List<Client>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var client = await _apiService.GetClientByIdAsync(id);
                if (client == null)
                {
                    return NotFound();
                }
                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading client details for ID {ClientId}", id);
                TempData["Error"] = "Failed to load client details.";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Email,Phone,Address,Region")] Client client)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var createdClient = await _apiService.CreateClientAsync(client);
                    if (createdClient != null)
                    {
                        _logger.LogInformation("Client created: {ClientName}", createdClient.Name);
                        TempData["Success"] = "Client created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Error"] = "Failed to create client.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating client");
                    ModelState.AddModelError("", $"Error creating client: {ex.Message}");
                }
            }
            return View(client);
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var client = await _apiService.GetClientByIdAsync(id);
                if (client == null)
                {
                    return NotFound();
                }
                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for client {ClientId}", id);
                TempData["Error"] = "Failed to load client for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClientId,Name,Email,Phone,Address,Region")] Client client)
        {
            if (id != client.ClientId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var updated = await _apiService.UpdateClientAsync(client);
                    if (updated)
                    {
                        _logger.LogInformation("Client updated: {ClientName}", client.Name);
                        TempData["Success"] = "Client updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Error"] = "Failed to update client.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating client {ClientId}", id);
                    ModelState.AddModelError("", $"Error updating client: {ex.Message}");
                }
            }
            return View(client);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = await _apiService.GetClientByIdAsync(id);
                if (client == null)
                {
                    return NotFound();
                }
                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete confirmation for client {ClientId}", id);
                TempData["Error"] = "Failed to load client for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var deleted = await _apiService.DeleteClientAsync(id);
                if (deleted)
                {
                    _logger.LogInformation("Client deleted: {ClientId}", id);
                    TempData["Success"] = "Client deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete client. Client may have existing contracts.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client {ClientId}", id);
                TempData["Error"] = $"Error deleting client: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}