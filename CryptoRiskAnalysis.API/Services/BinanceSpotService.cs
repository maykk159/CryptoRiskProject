using CryptoRiskAnalysis.API.Interfaces;
using CryptoRiskAnalysis.API.Models;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CryptoRiskAnalysis.API.Services
{
    public class BinanceSpotService : ICryptoDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BinanceSpotService> _logger;
        private const string BaseUrl = "https://api.binance.com/api/v3";
        private const int CacheDurationSeconds = 60; // 1 minute cache for fresh data

        public BinanceSpotService(HttpClient httpClient, IMemoryCache cache, ILogger<BinanceSpotService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Fetches price history and volume data from Binance klines endpoint.
        /// HTTP retries (on 5xx and 429) are handled automatically by the Polly policy
        /// configured in ServiceCollectionExtensions — no manual retry loop needed here.
        /// </summary>
        public async Task<(List<PriceData> priceHistory, decimal currentVolume, decimal avgVolume)> GetAllMarketDataAsync(string assetId, int days)
        {
            // 1. Map CoinGecko ID to Binance symbol
            var symbol = BinanceSymbolMapper.GetBinanceSymbol(assetId);
            if (symbol == null)
                throw new Exception($"Asset '{assetId}' not available on Binance");

            // 2. Check cache first (1-minute cache for fresh data)
            string cacheKey = $"binance_{symbol}_{days}";
            if (_cache.TryGetValue(cacheKey, out (List<PriceData>, decimal, decimal) cachedData))
            {
                _logger.LogDebug("Binance Cache HIT for {AssetId} ({Symbol})", assetId, symbol);
                return cachedData;
            }

            _logger.LogInformation("Binance Cache MISS for {AssetId} ({Symbol}) — fetching from API", assetId, symbol);

            // 3. Determine interval and limit based on time range
            var (interval, limit) = GetKlineParams(days);
            var url = $"{BaseUrl}/klines?symbol={symbol}&interval={interval}&limit={limit}";

            // 4. Fetch — Polly retries on transient errors and 429 automatically
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var klines = JsonSerializer.Deserialize<List<List<JsonElement>>>(content);

            if (klines == null || klines.Count == 0)
                throw new Exception($"No kline data returned for {symbol}");

            // Filter out any malformed candles to prevent IndexOutOfRangeException
            var validKlines = klines.Where(k => k.Count >= 6).ToList();
            if (validKlines.Count == 0)
                throw new Exception($"No valid kline data returned for {symbol}");

            // 5. Parse klines into PriceData
            // Binance returns: [timestamp(number), open(string), high(string), low(string), close(string), volume(string), ...]
            var priceHistory = validKlines.Select(k => new PriceData
            {
                // Timestamp is a number
                Timestamp = k[0].ValueKind == JsonValueKind.Number
                    ? k[0].GetInt64()
                    : long.Parse(k[0].GetString()!),
                // Close price is index 4 — use InvariantCulture to handle decimal points correctly
                Price = k[4].ValueKind == JsonValueKind.String
                    ? decimal.Parse(k[4].GetString()!, System.Globalization.CultureInfo.InvariantCulture)
                    : k[4].GetDecimal()
            }).OrderBy(p => p.Timestamp).ToList();

            // 6. Calculate volume metrics (volume is at index 5)
            // Use the last COMPLETED candle (index ^2) — not the live candle which starts at 0.
            var volumes = validKlines.Select(k =>
                k[5].ValueKind == JsonValueKind.String
                    ? decimal.Parse(k[5].GetString()!, System.Globalization.CultureInfo.InvariantCulture)
                    : k[5].GetDecimal()
            ).ToList();

            decimal currentVolume;
            decimal avgVolume;

            if (volumes.Count >= 2)
            {
                currentVolume = volumes[volumes.Count - 2]; // last completed candle
                avgVolume = volumes.Take(volumes.Count - 1).Average();
                _logger.LogInformation("Volume for {Symbol}: LastCompleted={Vol:F0}, Avg={Avg:F0}", symbol, currentVolume, avgVolume);
            }
            else
            {
                currentVolume = volumes.Last();
                avgVolume = currentVolume;
            }

            var result = (priceHistory, currentVolume, avgVolume);

            // 7. Cache for 1 minute
            _cache.Set(cacheKey, result, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(CacheDurationSeconds)));

            _logger.LogInformation("Binance: Fetched {Count} candles for {AssetId} ({Symbol}) — cached for {Duration}s",
                priceHistory.Count, assetId, symbol, CacheDurationSeconds);

            return result;
        }

        /// <summary>
        /// Determines the optimal kline interval and limit based on time range.
        /// Balances data granularity with API efficiency.
        /// </summary>
        private static (string interval, int limit) GetKlineParams(int days)
        {
            return days switch
            {
                7 => ("1d", 7),        // 7 days
                30 => ("1d", 30),      // 30 days
                90 => ("1d", 90),      // 90 days
                _ => ("1d", days)      // Default: daily candles
            };
        }

        // Legacy methods (not used in optimized flow, but required by interface)
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
    }
}
