using CryptoRiskAnalysis.API.Interfaces;
using CryptoRiskAnalysis.API.Models;
using System.Text.Json;

namespace CryptoRiskAnalysis.API.Services
{
    public class CoinGeckoService : ICryptoDataService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.coingecko.com/api/v3";

        public CoinGeckoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<PriceData>> GetHistoricalPriceDataAsync(string assetId, int days)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/coins/{assetId}/market_chart?vs_currency=usd&days={days}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<CoinGeckoMarketChart>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (data?.Prices == null) return new List<PriceData>();

                return data.Prices.Select(p => new PriceData
                {
                    Timestamp = (long)p[0],
                    Price = (decimal)p[1]
                }).ToList();
            }
            catch (Exception ex)
            {
                // Graceful handling: return empty list or throw custom exception
                Console.WriteLine($"Error fetching data: {ex.Message}");
                return new List<PriceData>();
            }
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
            public List<List<double>> Prices { get; set; }
            public List<List<double>> Total_Volumes { get; set; }
        }
    }
}
