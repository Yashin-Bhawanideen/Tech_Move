using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

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
        public async Task GetServiceRequests_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/servicerequests");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetServiceRequests_ReturnsJsonContent()
        {
            // Act
            var response = await _client.GetAsync("/api/servicerequests");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Is.Not.Null);
            Assert.That(content, Is.Not.Empty);
        }

        [Test]
        public async Task GetServiceRequests_WithStatusFilter_ReturnsFilteredResults()
        {
            // Act
            var response = await _client.GetAsync("/api/servicerequests?status=Pending");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Does.Contain("Pending"));
        }

        [Test]
        public async Task GetServiceRequestById_WithValidId_ReturnsRequest()
        {
            // Act
            var response = await _client.GetAsync("/api/servicerequests/1");

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
            var newRequest = new
            {
                contractId = 1,
                requestTitle = "Integration Test Request",
                description = "This is a test request from integration tests",
                amountUSD = 500.00m
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/servicerequests", newRequest);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async Task CreateServiceRequest_WithInvalidContract_ReturnsBadRequest()
        {
            // Arrange - contract doesn't exist
            var newRequest = new
            {
                contractId = 999,
                requestTitle = "Invalid Request",
                description = "This should fail",
                amountUSD = 500.00m
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/servicerequests", newRequest);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task UpdateRequestStatus_WithValidData_ReturnsOk()
        {
            // Arrange
            var statusUpdate = "Completed";

            // Act
            var response = await _client.PatchAsJsonAsync("/api/servicerequests/1/status", statusUpdate);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task UpdateRequestStatus_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var statusUpdate = "Completed";

            // Act
            var response = await _client.PatchAsJsonAsync("/api/servicerequests/999/status", statusUpdate);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task UpdateRequestStatus_WithInvalidStatus_ReturnsBadRequest()
        {
            // Arrange
            var statusUpdate = "InvalidStatus";

            // Act
            var response = await _client.PatchAsJsonAsync("/api/servicerequests/1/status", statusUpdate);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetExchangeRate_ReturnsValidRate()
        {
            // Act
            var response = await _client.GetAsync("/api/servicerequests/exchangerate");
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Is.Not.Null);
            Assert.That(content.ContainsKey("rate"), Is.True);

            if (content.ContainsKey("rate"))
            {
                var rate = Convert.ToDecimal(content["rate"]);
                Assert.That(rate, Is.GreaterThan(0));
            }
        }

        [Test]
        public async Task ConvertCurrency_ValidAmount_ReturnsConvertedValue()
        {
            // Act
            var response = await _client.GetAsync("/api/servicerequests/convert?usdAmount=100");
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Is.Not.Null);
            Assert.That(content.ContainsKey("zar"), Is.True);

            if (content.ContainsKey("zar"))
            {
                var zarAmount = Convert.ToDecimal(content["zar"]);
                Assert.That(zarAmount, Is.GreaterThan(0));
            }
        }

        [Test]
        public async Task ConvertCurrency_ZeroAmount_ReturnsZero()
        {
            // Act
            var response = await _client.GetAsync("/api/servicerequests/convert?usdAmount=0");
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            if (content.ContainsKey("zar"))
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
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            if (content.ContainsKey("zar"))
            {
                var zarAmount = Convert.ToDecimal(content["zar"]);
                Assert.That(zarAmount, Is.LessThan(0));
            }
        }
    }
}