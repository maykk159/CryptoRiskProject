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
        /// Fetches price history and volume data from Binance klines endpoint
        /// </summary>
        public async Task<(List<PriceData> priceHistory, decimal currentVolume, decimal avgVolume)> GetAllMarketDataAsync(string assetId, int days)
        {
            // 1. Map CoinGecko ID to Binance symbol
            var symbol = BinanceSymbolMapper.GetBinanceSymbol(assetId);
            if (symbol == null)
            {
                throw new Exception($"Asset '{assetId}' not available on Binance");
            }

            // 2. Check cache first (1-minute cache for fresh data)
            string cacheKey = $"binance_{symbol}_{days}";
            if (_cache.TryGetValue(cacheKey, out (List<PriceData>, decimal, decimal) cachedData))
            {
                _logger.LogDebug("Binance Cache HIT for {AssetId} ({Symbol}) - returning cached data", assetId, symbol);
                return cachedData;
            }

            _logger.LogInformation("Binance Cache MISS for {AssetId} ({Symbol}) - fetching from Binance API", assetId, symbol);

            // 3. Determine interval and limit based on time range
            var (interval, limit) = GetKlineParams(days);

            int maxRetries = 3;
            int baseRetryDelay = 1000; // 1 second base delay

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    if (attempt > 0)
                    {
                        int delay = baseRetryDelay * (attempt + 1);
                        _logger.LogWarning("Binance retry {Attempt}, waiting {Delay}ms for {Symbol}", attempt + 1, delay, symbol);
                        await Task.Delay(delay);
                    }

                    // 4. Fetch klines from Binance
                    var url = $"{BaseUrl}/klines?symbol={symbol}&interval={interval}&limit={limit}";
                    var response = await _httpClient.GetAsync(url);

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        _logger.LogWarning("Binance rate limit hit for {Symbol}, attempt {Attempt}/{MaxRetries}", symbol, attempt + 1, maxRetries);
                        if (attempt < maxRetries - 1) continue;
                        throw new Exception("Binance rate limit exceeded");
                    }

                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var klines = JsonSerializer.Deserialize<List<List<JsonElement>>>(content);

                    if (klines == null || !klines.Any())
                    {
                        throw new Exception($"No kline data returned for {symbol}");
                    }

                    // DEBUG: Log first kline to verify structure and types
                    if (klines.Count > 0 && _logger.IsEnabled(LogLevel.Debug))
                    {
                        var firstKline = klines[0];
                        _logger.LogDebug("First kline has {Count} elements", firstKline.Count);
                        _logger.LogDebug("Element[0] type: {Type}, value: {Value}", firstKline[0].ValueKind, firstKline[0]);
                        _logger.LogDebug("Element[1] type: {Type}, value: {Value}", firstKline[1].ValueKind, firstKline[1]);
                        _logger.LogDebug("Element[4] type: {Type}, value: {Value}", firstKline[4].ValueKind, firstKline[4]);
                        
                        // Parse and log the first price to debug the issue
                        var priceString = firstKline[4].ValueKind == JsonValueKind.String 
                            ? firstKline[4].GetString()! 
                            : firstKline[4].ToString();
                        var parsedPrice = decimal.Parse(priceString, System.Globalization.CultureInfo.InvariantCulture);
                        _logger.LogDebug("Price string: '{PriceString}', Parsed decimal: {ParsedPrice}", priceString, parsedPrice);
                    }

                    // 5. Parse klines into PriceData
                    // Binance returns: [timestamp(number), open(string), high(string), low(string), close(string), volume(string), ...]
                    var priceHistory = klines.Select(k => new PriceData
                    {
                        // Timestamp is a number
                        Timestamp = k[0].ValueKind == JsonValueKind.Number 
                            ? k[0].GetInt64() 
                            : long.Parse(k[0].GetString()!),
                        // Close price is index 4 and is a string - use InvariantCulture to handle decimal points correctly
                        Price = k[4].ValueKind == JsonValueKind.String
                            ? decimal.Parse(k[4].GetString()!, System.Globalization.CultureInfo.InvariantCulture)
                            : k[4].GetDecimal()
                    }).ToList();

                    // 6. Calculate volume metrics (volume is at index 5)
                    var volumes = klines.Select(k => 
                        k[5].ValueKind == JsonValueKind.String
                            ? decimal.Parse(k[5].GetString()!, System.Globalization.CultureInfo.InvariantCulture)
                            : k[5].GetDecimal()
                    ).ToList();

                    // FIX: For "Current Volume" risk analysis, we shouldn't use the LIVE incomplete candle
                    // because it starts at 0 every day/4h, causing massive "Low Volume" alerts.
                    // We use the LAST COMPLETED candle (Yesterday/Last Period) for a fair comparison.
                    
                    decimal currentVolume;
                    decimal avgVolume;

                    if (volumes.Count >= 2)
                    {
                        // Use the last completed candle (index ^2)
                        currentVolume = volumes[volumes.Count - 2];
                        // Average should ideally include the completed candles
                        avgVolume = volumes.Take(volumes.Count - 1).Average();
                        
                        _logger.LogInformation("Volume Analysis for {Symbol}: Last Completed Vol={Vol}, Avg(30d)={Avg}", symbol, currentVolume, avgVolume);
                    }
                    else
                    {
                        // Fallback if only 1 candle exists
                        currentVolume = volumes.Last();
                        avgVolume = currentVolume;
                    }

                    var result = (priceHistory, currentVolume, avgVolume);

                    // 7. Cache for 1 minute
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(CacheDurationSeconds));
                    _cache.Set(cacheKey, result, cacheOptions);

                    _logger.LogInformation("Binance: Fetched {Count} candles for {AssetId} ({Symbol}) - Cached for {Duration}s", priceHistory.Count, assetId, symbol, CacheDurationSeconds);
                    return result;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Binance HTTP error for {Symbol} (attempt {Attempt}/{MaxRetries})", symbol, attempt + 1, maxRetries);
                    if (attempt == maxRetries - 1) throw;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Binance JSON parsing error for {Symbol}", symbol);
                    throw; // Don't retry JSON errors
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Binance error for {Symbol} (attempt {Attempt}/{MaxRetries})", symbol, attempt + 1, maxRetries);
                    if (attempt == maxRetries - 1) throw;
                }
            }

            throw new Exception($"Failed to fetch data from Binance for {symbol} after {maxRetries} attempts");
        }

        /// <summary>
        /// Determines the optimal kline interval and limit based on time range
        /// Balances data granularity with API efficiency
        /// </summary>
        private (string interval, int limit) GetKlineParams(int days)
        {
            return days switch
            {
                7 => ("1d", 7),        // 7 days
                30 => ("1d", 30),      // 30 days (Standardized to Daily for accurate Vol/Vol calc)
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
