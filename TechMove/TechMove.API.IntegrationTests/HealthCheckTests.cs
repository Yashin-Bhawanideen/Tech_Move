using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace TechMove.API.IntegrationTests
{
    [TestFixture]
    public class HealthCheckTests
    {
        private CustomWebApplicationFactory _factory;
        private HttpClient _client;

        [SetUp]
        public void Setup()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task ApiRoot_ReturnsNotFound_ButServerIsRunning()
        {
            // Act
            var response = await _client.GetAsync("/");

            // Assert - API root might return 404, but that's fine as long as server responds
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task SwaggerEndpoint_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/swagger/v1/swagger.json");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task ApiIsAccessible_ReturnsResponse()
        {
            // Act
            var response = await _client.GetAsync("/api/contracts");

            // Assert - Should be unauthorized (needs auth) but endpoint exists
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }
}