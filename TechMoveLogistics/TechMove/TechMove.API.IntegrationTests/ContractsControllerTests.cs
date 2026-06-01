using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using TechMove.API.DTOs;

namespace TechMove.API.IntegrationTests
{
    [TestFixture]
    public class ContractsControllerTests
    {
        private CustomWebApplicationFactory _factory;
        private HttpClient _client;
        private string _authToken;

        [SetUp]
        public async Task Setup()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
            _authToken = await GetAuthToken();

            if (!string.IsNullOrEmpty(_authToken))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
            }
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        private async Task<string> GetAuthToken()
        {
            var loginData = new { username = "testuser", password = "testpass" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                return content?["token"];
            }
            return null;
        }

        [Test]
        public async Task GetContracts_ReturnsSuccess()
        {
            var response = await _client.GetAsync("/api/contracts");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task CreateContract_WithValidData_ReturnsCreated()
        {
            var newContract = new CreateContractDto
            {
                ClientId = 1,
                ContractNumber = "TEST-002",
                ServiceLead = "Test Lead",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(365),
                Status = "Active"
            };

            var response = await _client.PostAsJsonAsync("/api/contracts", newContract);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async Task GetContractById_WithInvalidId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/contracts/999");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task UpdateContractStatus_WithValidData_ReturnsOk()
        {
            var statusUpdate = new { status = "Expired" };
            var response = await _client.PatchAsJsonAsync("/api/contracts/1/status", statusUpdate);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetActiveContracts_ReturnsSuccess()
        {
            var response = await _client.GetAsync("/api/contracts/active");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}