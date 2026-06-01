using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace TechMove.API.Services
{
    public class CurrencyExchangeService : ICurrencyExchangeService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CurrencyExchangeService> _logger;
        private const string CACHE_KEY = "USD_ZAR_RATE";
        private const int CACHE_DURATION_MINUTES = 60;

        public CurrencyExchangeService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<CurrencyExchangeService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current USD to ZAR exchange rate from external API
        /// Uses caching to avoid excessive API calls
        /// </summary>
        public async Task<decimal> GetUSDtoZARRateAsync()
        {
            // Try to get from cache first
            if (_cache.TryGetValue(CACHE_KEY, out decimal cachedRate))
            {
                _logger.LogInformation("Using cached exchange rate: {Rate}", cachedRate);
                return cachedRate;
            }

            try
            {
                _logger.LogInformation("Fetching latest exchange rate from API");

                // Using ExchangeRate-API (free tier, no API key required)
                var apiUrl = "https://api.exchangerate-api.com/v4/latest/USD";

                var response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var exchangeData = JsonSerializer.Deserialize<ExchangeRateResponse>(jsonString);

                if (exchangeData?.Rates != null && exchangeData.Rates.TryGetValue("ZAR", out var rate))
                {
                    var zarRate = (decimal)rate;

                    // Cache the result for 1 hour
                    _cache.Set(CACHE_KEY, zarRate, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));

                    _logger.LogInformation("Successfully fetched USD to ZAR rate: {Rate}", zarRate);
                    return zarRate;
                }

                _logger.LogWarning("ZAR rate not found in API response, using fallback rate");
                return GetFallbackRate();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching exchange rate from API");
                return GetFallbackRate();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error fetching exchange rate");
                return GetFallbackRate();
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout error fetching exchange rate");
                return GetFallbackRate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching exchange rate");
                return GetFallbackRate();
            }
        }

        /// <summary>
        /// Converts USD amount to ZAR using current exchange rate
        /// </summary>
        public async Task<decimal> ConvertUSDtoZARAsync(decimal usdAmount)
        {
            // Validate input
            if (usdAmount < 0)
                throw new ArgumentException("USD amount cannot be negative", nameof(usdAmount));

            if (usdAmount == 0)
                return 0;

            // Get current exchange rate
            var rate = await GetUSDtoZARRateAsync();

            // Calculate and round to 2 decimal places
            var result = Math.Round(usdAmount * rate, 2, MidpointRounding.AwayFromZero);

            _logger.LogDebug("Converted {USD} USD to {ZAR} ZAR using rate {Rate}", usdAmount, result, rate);
            return result;
        }

        /// <summary>
        /// Returns fallback rate when API is unavailable
        /// </summary>
        private decimal GetFallbackRate()
        {
            const decimal fallbackRate = 18.50m;
            _logger.LogWarning("Using fallback exchange rate: {Rate}", fallbackRate);
            return fallbackRate;
        }

        /// <summary>
        /// Response model for the ExchangeRate-API
        /// </summary>
        private class ExchangeRateResponse
        {
            public string? Base { get; set; }
            public Dictionary<string, double>? Rates { get; set; }
            public DateTime Date { get; set; }
        }
    }
}