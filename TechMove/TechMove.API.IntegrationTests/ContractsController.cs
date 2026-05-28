using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

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
            _authToken = await TestHelper.GetAuthToken(_client);

            if (!string.IsNullOrEmpty(_authToken))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
            }
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task GetContracts_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange - remove auth header
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync("/api/contracts");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task GetContracts_WithAuth_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/contracts");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetContracts_ReturnsJsonContent()
        {
            // Act
            var response = await _client.GetAsync("/api/contracts");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Is.Not.Null);
            Assert.That(content, Is.Not.Empty);
        }

        [Test]
        public async Task GetContracts_WithStatusFilterActive_ReturnsFilteredResults()
        {
            // Act
            var response = await _client.GetAsync("/api/contracts?status=Active");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Does.Contain("Active"));
        }

        [Test]
        public async Task GetContracts_WithStatusFilterExpired_ReturnsFilteredResults()
        {
            // Act
            var response = await _client.GetAsync("/api/contracts?status=Expired");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Does.Contain("Expired"));
        }

        [Test]
        public async Task GetContractById_WithValidId_ReturnsContract()
        {
            // Act
            var response = await _client.GetAsync("/api/contracts/1");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetContractById_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/contracts/999");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task CreateContract_WithValidData_ReturnsCreated()
        {
            // Arrange
            var newContract = new
            {
                clientId = 1,
                contractNumber = "CON-004",
                serviceLead = "Test Lead",
                startDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
                endDate = DateTime.UtcNow.AddDays(365).ToString("yyyy-MM-dd"),
                status = "Active",
                termsAndConditions = "Test terms for integration test"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/contracts", newContract);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async Task CreateContract_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange - missing required fields
            var invalidContract = new { };

            // Act
            var response = await _client.PostAsJsonAsync("/api/contracts", invalidContract);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task UpdateContractStatus_WithValidData_ReturnsOk()
        {
            // Arrange
            var statusUpdate = new { status = "Expired" };

            // Act
            var response = await _client.PatchAsJsonAsync("/api/contracts/1/status", statusUpdate);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task UpdateContractStatus_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var statusUpdate = new { status = "Expired" };

            // Act
            var response = await _client.PatchAsJsonAsync("/api/contracts/999/status", statusUpdate);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task UpdateContractStatus_WithInvalidStatus_ReturnsBadRequest()
        {
            // Arrange
            var statusUpdate = new { status = "InvalidStatus" };

            // Act
            var response = await _client.PatchAsJsonAsync("/api/contracts/1/status", statusUpdate);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetActiveContracts_ReturnsOnlyActiveContracts()
        {
            // Act
            var response = await _client.GetAsync("/api/contracts/active");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Does.Contain("Active"));
        }
    }
}