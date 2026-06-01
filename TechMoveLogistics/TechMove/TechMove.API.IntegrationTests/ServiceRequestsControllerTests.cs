using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using TechMove.API.DTOs;

namespace TechMove.API.IntegrationTests
{
    [TestFixture]
    public class ServiceRequestsControllerTests
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
            // Clean up resources after each test
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
        public async Task GetServiceRequests_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/api/servicerequests");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetServiceRequestById_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/servicerequests/999");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task CreateServiceRequest_WithValidData_ReturnsCreated()
        {
            // Arrange
            var newRequest = new CreateServiceRequestDto
            {
                ContractId = 1,
                RequestTitle = "Integration Test Request",
                Description = "This is a test request from integration tests",
                AmountUSD = 500.00m
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/servicerequests", newRequest);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async Task CreateServiceRequest_WithInvalidContract_ReturnsBadRequest()
        {
            // Arrange
            var newRequest = new CreateServiceRequestDto
            {
                ContractId = 999,
                RequestTitle = "Invalid Request",
                Description = "This should fail",
                AmountUSD = 500.00m
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/servicerequests", newRequest);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task UpdateRequestStatus_WithValidData_ReturnsOk()
        {
            // Arrange - First create a request
            var newRequest = new CreateServiceRequestDto
            {
                ContractId = 1,
                RequestTitle = "Status Test Request",
                Description = "Testing status update",
                AmountUSD = 100.00m
            };

            var createResponse = await _client.PostAsJsonAsync("/api/servicerequests", newRequest);
            var createdRequest = await createResponse.Content.ReadFromJsonAsync<ServiceRequestDto>();

            // Act - Update status
            var statusUpdate = new { status = "InProgress" };
            var response = await _client.PatchAsJsonAsync($"/api/servicerequests/{createdRequest.ServiceRequestId}/status", statusUpdate);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task UpdateRequestStatus_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var statusUpdate = new { status = "InProgress" };

            // Act
            var response = await _client.PatchAsJsonAsync("/api/servicerequests/999/status", statusUpdate);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task GetExchangeRate_ReturnsValidRate()
        {
            // Act
            var response = await _client.GetAsync("/api/servicerequests/exchangerate");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            Assert.That(content, Is.Not.Null);
            Assert.That(content.ContainsKey("rate"), Is.True);
        }

        [Test]
        public async Task ConvertCurrency_ValidAmount_ReturnsConvertedValue()
        {
            // Act
            var response = await _client.GetAsync("/api/servicerequests/convert?usdAmount=100");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            Assert.That(content, Is.Not.Null);
            Assert.That(content.ContainsKey("zar"), Is.True);
        }

        [Test]
        public async Task ConvertCurrency_ZeroAmount_ReturnsZero()
        {
            // Act
            var response = await _client.GetAsync("/api/servicerequests/convert?usdAmount=0");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            if (content != null && content.ContainsKey("zar"))
            {
                var zarAmount = Convert.ToDecimal(content["zar"]);
                Assert.That(zarAmount, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task ConvertCurrency_NegativeAmount_ReturnsNegativeValue()
        {
            // Act
            var response = await _client.GetAsync("/api/servicerequests/convert?usdAmount=-100");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            if (content != null && content.ContainsKey("zar"))
            {
                var zarAmount = Convert.ToDecimal(content["zar"]);
                Assert.That(zarAmount, Is.LessThan(0));
            }
        }
    }
}