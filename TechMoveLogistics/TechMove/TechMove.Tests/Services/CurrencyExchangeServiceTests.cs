using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using TechMove.Services;

namespace TechMove.Tests.Services
{
    [TestFixture]
    public class CurrencyExchangeServiceTests
    {
        private Mock<ILogger<CurrencyExchangeService>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<CurrencyExchangeService>>();
        }

        [Test]
        public async Task ConvertUSDtoZARAsync_SmallDecimalAmount_RoundsCorrectly()
        {
            // Arrange
            var expectedRate = 18.1234m;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            // Create the response with the exact rate
            var responseContent = $"{{\"rates\": {{\"ZAR\": {expectedRate.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}}}";

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new CurrencyExchangeService(httpClient, cache, _loggerMock.Object);
            var usdAmount = 1.23m;

            
            var expectedZarAmount = 22.29m;

            // Act
            var result = await service.ConvertUSDtoZARAsync(usdAmount);

            // Assert
            Assert.That(result, Is.EqualTo(expectedZarAmount),
                $"Expected: {expectedZarAmount}, Actual: {result}, Rate: {expectedRate}");
        }

        [Test]
        public async Task ConvertUSDtoZARAsync_ValidAmount_ReturnsCorrectConvertedAmount()
        {
            // Arrange
            var expectedRate = 18.50m;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseContent = $"{{\"rates\": {{\"ZAR\": {expectedRate.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}}}";

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new CurrencyExchangeService(httpClient, cache, _loggerMock.Object);
            var usdAmount = 100m;
            var expectedZarAmount = 1850.00m;

            // Act
            var result = await service.ConvertUSDtoZARAsync(usdAmount);

            // Assert
            Assert.That(result, Is.EqualTo(expectedZarAmount));
        }

        [Test]
        public async Task ConvertUSDtoZARAsync_ZeroAmount_ReturnsZero()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new CurrencyExchangeService(httpClient, cache, _loggerMock.Object);

            // Act
            var result = await service.ConvertUSDtoZARAsync(0);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void ConvertUSDtoZARAsync_NegativeAmount_ThrowsArgumentException()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new CurrencyExchangeService(httpClient, cache, _loggerMock.Object);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await service.ConvertUSDtoZARAsync(-100));

            Assert.That(exception.Message, Does.Contain("USD amount cannot be negative"));
        }

        [Test]
        public async Task GetUSDtoZARRateAsync_ApiSuccess_ReturnsCorrectRate()
        {
            // Arrange
            var expectedRate = 18.50m;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseContent = $"{{\"rates\": {{\"ZAR\": {expectedRate.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}}}";

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new CurrencyExchangeService(httpClient, cache, _loggerMock.Object);

            // Act
            var result = await service.GetUSDtoZARRateAsync();

            // Assert
            Assert.That(result, Is.EqualTo(expectedRate));
        }

        [Test]
        public async Task GetUSDtoZARRateAsync_ApiFailure_ReturnsFallbackRate()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("API is down"));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new CurrencyExchangeService(httpClient, cache, _loggerMock.Object);

            // Act
            var result = await service.GetUSDtoZARRateAsync();

            // Assert
            Assert.That(result, Is.EqualTo(18.50m)); 
        }

        [Test]
        public async Task ConvertUSDtoZARAsync_MultipleRatesTest()
        {
            // Test with rate 18.50
            using (var cache1 = new MemoryCache(new MemoryCacheOptions()))
            {
                var mockHandler1 = new Mock<HttpMessageHandler>();
                mockHandler1.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("{\"rates\": {\"ZAR\": 18.50}}")
                    });

                var httpClient1 = new HttpClient(mockHandler1.Object);
                var service1 = new CurrencyExchangeService(httpClient1, cache1, _loggerMock.Object);
                var result1 = await service1.ConvertUSDtoZARAsync(1.23m);
                Assert.That(result1, Is.EqualTo(22.76m), "Failed with rate 18.50");
            }

            // Test with rate 18.1234 (fresh cache)
            using (var cache2 = new MemoryCache(new MemoryCacheOptions()))
            {
                var mockHandler2 = new Mock<HttpMessageHandler>();
                mockHandler2.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("{\"rates\": {\"ZAR\": 18.1234}}")
                    });

                var httpClient2 = new HttpClient(mockHandler2.Object);
                var service2 = new CurrencyExchangeService(httpClient2, cache2, _loggerMock.Object);
                var result2 = await service2.ConvertUSDtoZARAsync(1.23m);
                Assert.That(result2, Is.EqualTo(22.29m), "Failed with rate 18.1234");
            }
        }

        [Test]
        public async Task ConvertUSDtoZARAsync_DirectCalculation_VerifiesRounding()
        {
            
            var rate = 18.1234m;
            var usdAmount = 1.23m;
            var calculated = usdAmount * rate;
            var rounded = Math.Round(calculated, 2, MidpointRounding.AwayFromZero);

            
            Assert.That(calculated, Is.EqualTo(22.291782m), "Raw calculation is wrong");
           
            Assert.That(rounded, Is.EqualTo(22.29m), "Rounding is wrong");
        }
    }
}
//References
//Kayal, S., 2026. Fundamentals of Unit Testing: Unit Testing of MVC Application. [Online]
//Available at: https://www.c-sharpcorner.com/UploadFile/dacca2/fundamentals-of-unit-testing-unit-testing-of-mvc-applicatio/
//microsoft, 2026.Creating Unit Tests for ASP.NET MVC Applications (C#). [Online] 
//Available at: https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions-1/unit-testing/creating-unit-tests-for-asp-net-mvc-applications-cs
//Reily, J., 2013.Unit testing MVC controllers / Mocking UrlHelper. [Online]
//Available at: https://johnnyreilly.com/unit-testing-mvc-controllers-mocking
//StrangeWill, 2011.Nunit Testing MVC Site. [Online]
//Available at: https://stackoverflow.com/questions/7476041/nunit-testing-mvc-site