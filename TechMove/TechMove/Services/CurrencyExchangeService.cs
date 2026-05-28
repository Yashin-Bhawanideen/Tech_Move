using Microsoft.Extensions.Caching.Memory;

using System.Text.Json;

namespace TechMove.Services
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


        public async Task<decimal> GetUSDtoZARRateAsync()
        {

            //try to get from cache
            if (_cache.TryGetValue(CACHE_KEY, out decimal cacheRate))
            {
                return cacheRate;
            }
            try
            {
                var apiUrl = "https://api.exchangerate-api.com/v4/latest/USD";

                var response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var exchangeData = JsonSerializer.Deserialize<ExchangeRateResponse>(jsonString);

                if (exchangeData?.Rates != null && exchangeData.Rates.TryGetValue("ZAR", out var rate))
                {
                    var zarRate = (decimal)rate;


                    _cache.Set(CACHE_KEY, zarRate, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));

                    _logger.LogInformation("Successfully fetched USD to ZAR rate: {Rate}", zarRate);
                    return zarRate;
                }
                throw new Exception("Failed to get ZAR rate from API response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rate from API");


                const decimal fallbackRate = 18.50m;
                _logger.LogWarning("Using fallback exchange rate: {Rate}", fallbackRate);
                return fallbackRate;

            }
        }


        public async Task<decimal> ConvertUSDtoZARAsync(decimal usdAmount)
        {
            if (usdAmount < 0)
                throw new ArgumentException("USD amount cannot be negative", nameof(usdAmount));

            var rate = await GetUSDtoZARRateAsync();
            return Math.Round(usdAmount * rate, 2);
        }

        private class ExchangeRateResponse
        {
            public Dictionary<string, double> Rates { get; set; }
        }


    }
}
