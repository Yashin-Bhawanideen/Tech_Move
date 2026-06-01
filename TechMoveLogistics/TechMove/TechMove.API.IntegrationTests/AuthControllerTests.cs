using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;

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
            _client?.Dispose();
            _factory?.Dispose();
        }

        [Test]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            var loginData = new { username = "testuser", password = "testpass" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Login_WithEmptyCredentials_ReturnsUnauthorized()
        {
            var loginData = new { username = "", password = "" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }
}