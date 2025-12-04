using CryptoRiskAnalysis.API.Models;

namespace CryptoRiskAnalysis.API.Interfaces
{
    public interface IRiskEngine
    {
        RiskScoreResult CalculateRisk(List<PriceData> priceHistory, decimal currentVolume, decimal averageVolume);
    }
}
