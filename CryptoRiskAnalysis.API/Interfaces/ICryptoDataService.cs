using CryptoRiskAnalysis.API.Models;

namespace CryptoRiskAnalysis.API.Interfaces
{
    public interface ICryptoDataService
    {
        // Optimized: Single API call for all data
        Task<(List<PriceData> priceHistory, decimal currentVolume, decimal avgVolume)> GetAllMarketDataAsync(string assetId, int days);
        
        // Legacy methods (kept for backward compatibility)
        Task<List<PriceData>> GetHistoricalPriceDataAsync(string assetId, int days);
        Task<decimal> GetCurrentVolumeAsync(string assetId);
        Task<decimal> GetAverageVolumeAsync(string assetId, int days);
    }
}
