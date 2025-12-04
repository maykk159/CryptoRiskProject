namespace CryptoRiskAnalysis.API.Models
{
    public class RiskScoreResult
    {
        public decimal VolatilityScore { get; set; }
        public decimal TrendScore { get; set; }
        public decimal VolumeScore { get; set; }
        public decimal CompositeRiskScore { get; set; }
        public List<PriceData> PriceHistory { get; set; } = new();
    }
}
