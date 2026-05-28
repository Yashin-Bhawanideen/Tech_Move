using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace TechMove.API.IntegrationTests
{

    [TestFixture]
    public class AuthControllerTests
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
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var loginData = new { username = "testuser", password = "testpass" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Is.Not.Null);
            Assert.That(content.ContainsKey("token"), Is.True);
            Assert.That(content["token"], Is.Not.Empty);
        }

        [Test]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginData = new { username = "invalid", password = "wrong" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Login_WithEmptyCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginData = new { username = "", password = "" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Login_WithNullCredentials_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", new { });

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }
}
