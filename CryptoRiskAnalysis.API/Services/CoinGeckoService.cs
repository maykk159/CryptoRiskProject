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
                    var data = JsonSerializer.Deserialize<CoinGeckoMarketChart>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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


        public async Task<List<PriceData>> GetHistoricalPriceDataAsync(string assetId, int days)
        {
            int maxRetries = 3;
            int baseRetryDelay = 2000; // 2 seconds base

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // Add delay to avoid rate limiting (2s, 4s, 6s)
                    if (i > 0)
                    {
                        int delay = baseRetryDelay * (i + 1);
                        Console.WriteLine($"Waiting {delay}ms before retry {i + 1}...");
                        await Task.Delay(delay);
                    }

                    var response = await _httpClient.GetAsync($"{BaseUrl}/coins/{assetId}/market_chart?vs_currency=usd&days={days}");
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        Console.WriteLine($"⚠️ Rate limit hit for {assetId}, attempt {i + 1}/{maxRetries}");
                        if (i < maxRetries - 1) continue; // Retry if not last attempt
                        return new List<PriceData>(); // Give up on last attempt
                    }

                    response.EnsureSuccessStatusCode();
                    
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<CoinGeckoMarketChart>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (data?.Prices == null) return new List<PriceData>();

                    Console.WriteLine($"✅ Successfully fetched {data.Prices.Count} price points for {assetId}");
                    return data.Prices.Select(p => new PriceData
                    {
                        Timestamp = (long)p[0],
                        Price = (decimal)p[1]
                    }).ToList();
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"❌ HTTP Error for {assetId} (attempt {i + 1}/{maxRetries}): {ex.Message}");
                    if (i == maxRetries - 1) return new List<PriceData>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error fetching {assetId} data (attempt {i + 1}/{maxRetries}): {ex.Message}");
                    if (i == maxRetries - 1) return new List<PriceData>();
                }
            }

            return new List<PriceData>();
        }

        public async Task<decimal> GetCurrentVolumeAsync(string assetId)
        {
             // For simplicity, we can get this from market_chart too (last point of total_volumes)
             // Or use /simple/price endpoint with include_24hr_vol
             // Let's use market_chart since we are already calling it? 
             // Actually, to save calls, maybe we should cache or just fetch once.
             // But the interface separates them. I'll implement fetching from market_chart for now to avoid rate limits of multiple calls.
             
             // Note: This implementation might be inefficient if called separately. 
             // In a real app, we would fetch all data at once.
             // For this prototype, I'll fetch market_chart for 1 day to get recent volume.
             
             try
             {
                var response = await _httpClient.GetAsync($"{BaseUrl}/coins/{assetId}/market_chart?vs_currency=usd&days=1");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<CoinGeckoMarketChart>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (data?.Total_Volumes == null || !data.Total_Volumes.Any()) return 0;
                
                return (decimal)data.Total_Volumes.Last()[1];
             }
             catch
             {
                 return 0;
             }
        }

        public async Task<decimal> GetAverageVolumeAsync(string assetId, int days)
        {
             try
             {
                var response = await _httpClient.GetAsync($"{BaseUrl}/coins/{assetId}/market_chart?vs_currency=usd&days={days}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<CoinGeckoMarketChart>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (data?.Total_Volumes == null || !data.Total_Volumes.Any()) return 0;
                
                var volumes = data.Total_Volumes.Select(v => (decimal)v[1]).ToList();
                return volumes.Average();
             }
             catch
             {
                 return 0;
             }
        }
        
        // Helper class for deserialization
        private class CoinGeckoMarketChart
        {
            public List<List<double>> Prices { get; set; } = new();
            public List<List<double>> Total_Volumes { get; set; } = new();
        }
    }
}
