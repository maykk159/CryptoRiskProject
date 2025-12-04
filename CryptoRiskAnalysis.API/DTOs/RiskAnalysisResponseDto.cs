using CryptoRiskAnalysis.API.Models;

namespace CryptoRiskAnalysis.API.DTOs
{
    public class RiskAnalysisResponseDto
    {
        public string AssetId { get; set; } = string.Empty;
        public decimal CompositeRiskScore { get; set; }
        public decimal VolatilityScore { get; set; }
        public decimal TrendScore { get; set; }
        public decimal VolumeScore { get; set; }
        public List<PriceData> PriceHistory { get; set; } = new();
    }
}
