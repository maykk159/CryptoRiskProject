using CryptoRiskAnalysis.API.Models;

namespace CryptoRiskAnalysis.API.Interfaces
{
    public interface ICryptoDataService
    {
        Task<List<PriceData>> GetHistoricalPriceDataAsync(string assetId, int days);
        Task<decimal> GetCurrentVolumeAsync(string assetId);
        Task<decimal> GetAverageVolumeAsync(string assetId, int days);
    }
}
