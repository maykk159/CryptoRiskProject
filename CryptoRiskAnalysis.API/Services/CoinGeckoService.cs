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

        // ✅ OPTIMIZED: Single API call with 60-second cache!
        public async Task<(List<PriceData> priceHistory, decimal currentVolume, decimal avgVolume)> GetAllMarketDataAsync(string assetId, int days)
        {
            string cacheKey = $"market_data_{assetId}_{days}";
            
            // 🚀 Check cache first!
            if (_cache.TryGetValue(cacheKey, out (List<PriceData>, decimal, decimal) cachedData))
            {
                _logger.LogDebug("CoinGecko Cache HIT for {AssetId} - returning cached data", assetId);
                return cachedData;
            }

            _logger.LogInformation("CoinGecko Cache MISS for {AssetId} - fetching from API", assetId);
            
            int maxRetries = 3;
            int baseRetryDelay = 2000;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (i > 0)
                    {
                        int delay = baseRetryDelay * (i + 1);
                        _logger.LogWarning("CoinGecko retry {Attempt}, waiting {Delay}ms for {AssetId}", i + 1, delay, assetId);
                        await Task.Delay(delay);
                    }

                    var response = await _httpClient.GetAsync($"{BaseUrl}/coins/{assetId}/market_chart?vs_currency=usd&days={days}");
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        _logger.LogWarning("CoinGecko rate limit hit for {AssetId}, attempt {Attempt}/{MaxRetries}", assetId, i + 1, maxRetries);
                        if (i < maxRetries - 1) continue;
                        return (new List<PriceData>(), 0, 0);
                    }

                    response.EnsureSuccessStatusCode();
                    
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<CoinGeckoMarketChart>(content, JsonOptions);

                    if (data?.Prices == null || data?.Total_Volumes == null)
                    {
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
                    var currentVolume = volumes.Any() ? volumes.Last() : 0;
                    var avgVolume = volumes.Any() ? volumes.Average() : 0;

                    var result = (priceHistory, currentVolume, avgVolume);
                    
                    // 💾 Store in cache for 3 minutes (180 seconds)
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(180));
                    _cache.Set(cacheKey, result, cacheOptions);

                    _logger.LogInformation("CoinGecko: Fetched {PriceCount} prices & {VolumeCount} volumes for {AssetId} - Cached for 3 minutes", priceHistory.Count, volumes.Count, assetId);
                    return result;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "CoinGecko HTTP Error for {AssetId} (attempt {Attempt}/{MaxRetries})", assetId, i + 1, maxRetries);
                    if (i == maxRetries - 1) return (new List<PriceData>(), 0, 0);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CoinGecko Error fetching {AssetId} (attempt {Attempt}/{MaxRetries})", assetId, i + 1, maxRetries);
                    if (i == maxRetries - 1) return (new List<PriceData>(), 0, 0);
                }
            }

            return (new List<PriceData>(), 0, 0);
        }

        // Legacy methods - now delegate to the cached GetAllMarketDataAsync
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
