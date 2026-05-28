namespace TechMove.API.Services
{
    public interface ICurrencyExchangeService
    {
        /// <summary>
        /// Gets the current USD to ZAR exchange rate
        /// </summary>
        /// <returns>Exchange rate as decimal</returns>
        Task<decimal> GetUSDtoZARRateAsync();

        /// <summary>
        /// Converts USD amount to ZAR using current exchange rate
        /// </summary>
        /// <param name="usdAmount">Amount in USD</param>
        /// <returns>Converted amount in ZAR (rounded to 2 decimal places)</returns>
        Task<decimal> ConvertUSDtoZARAsync(decimal usdAmount);
    }
}