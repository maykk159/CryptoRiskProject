using CryptoRiskAnalysis.API.Interfaces;
using CryptoRiskAnalysis.API.Models;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CryptoRiskAnalysis.API.Services
{
    public class CoinGeckoService : ICryptoDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CoinGeckoService> _logger;
        private const string BaseUrl = "https://api.coingecko.com/api/v3";
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public CoinGeckoService(HttpClient httpClient, IMemoryCache cache, ILogger<CoinGeckoService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Fetches price history and volume from CoinGecko with in-memory caching.
        /// HTTP retries (on 5xx and 429) are handled by the Polly policy in ServiceCollectionExtensions.
        /// Previously: 429 was silently returning an empty list, hiding the error from the caller.
        /// Now: EnsureSuccessStatusCode() throws on any non-2xx, letting Polly retry and the
        ///      ExceptionHandlingMiddleware return a proper error response.
        /// </summary>
        public async Task<(List<PriceData> priceHistory, decimal currentVolume, decimal avgVolume)> GetAllMarketDataAsync(string assetId, int days)
        {
            string cacheKey = $"market_data_{assetId}_{days}";

            // Check cache first
            if (_cache.TryGetValue(cacheKey, out (List<PriceData>, decimal, decimal) cachedData))
            {
                _logger.LogDebug("CoinGecko Cache HIT for {AssetId}", assetId);
                return cachedData;
            }

            _logger.LogInformation("CoinGecko Cache MISS for {AssetId} — fetching from API", assetId);

            // Polly handles retries on 429 and transient errors — no manual loop needed
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/coins/{assetId}/market_chart?vs_currency=usd&days={days}");

            response.EnsureSuccessStatusCode(); // throws HttpRequestException on any non-2xx (incl. 429 if Polly exhausted)

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<CoinGeckoMarketChart>(content, JsonOptions);

            if (data?.Prices == null || data.Total_Volumes == null)
            {
                _logger.LogWarning("CoinGecko returned null or empty data for {AssetId}", assetId);
                return (new List<PriceData>(), 0, 0);
            }

            // Extract price history
            var priceHistory = data.Prices.Select(p => new PriceData
            {
                Timestamp = (long)p[0],
                Price = (decimal)p[1]
            }).ToList();

            // Extract volumes
            var volumes = data.Total_Volumes.Select(v => (decimal)v[1]).ToList();
            var currentVolume = volumes.Count > 0 ? volumes.Last() : 0;
            var avgVolume = volumes.Count > 0 ? volumes.Average() : 0;

            var result = (priceHistory, currentVolume, avgVolume);

            // Cache for 3 minutes (CoinGecko rate limits are stricter than Binance)
            _cache.Set(cacheKey, result, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(180)));

            _logger.LogInformation("CoinGecko: Fetched {PriceCount} prices for {AssetId} — cached for 3 minutes",
                priceHistory.Count, assetId);

            return result;
        }

        // Legacy methods — not used in optimized flow, but required by interface
        public async Task<List<PriceData>> GetHistoricalPriceDataAsync(string assetId, int days)
        {
            var (priceHistory, _, _) = await GetAllMarketDataAsync(assetId, days);
            return priceHistory;
        }

        public async Task<decimal> GetCurrentVolumeAsync(string assetId)
        {
            var (_, currentVolume, _) = await GetAllMarketDataAsync(assetId, 1);
            return currentVolume;
        }

        public async Task<decimal> GetAverageVolumeAsync(string assetId, int days)
        {
            var (_, _, avgVolume) = await GetAllMarketDataAsync(assetId, days);
            return avgVolume;
        }

        // Helper class for deserialization
        private class CoinGeckoMarketChart
        {
            public List<List<double>> Prices { get; set; } = new();
            public List<List<double>> Total_Volumes { get; set; } = new();
        }
    }
}
