using CryptoRiskAnalysis.API.Interfaces;
using CryptoRiskAnalysis.API.Models;

namespace CryptoRiskAnalysis.API.Services
{
    public class RiskAnalysisEngine : IRiskEngine
    {
        public RiskScoreResult CalculateRisk(List<PriceData> priceHistory, decimal currentVolume, decimal averageVolume)
        {
            if (priceHistory == null || !priceHistory.Any())
            {
                return new RiskScoreResult();
            }

            // 1. Volatility (Standard Deviation)
            var prices = priceHistory.Select(p => p.Price).ToList();
            var mean = prices.Average();
            var sumOfSquares = prices.Sum(p => Math.Pow((double)(p - mean), 2));
            var stdDev = Math.Sqrt(sumOfSquares / prices.Count);
            
            // Normalize StdDev as percentage of mean
            var volatilityPercent = (decimal)stdDev / mean;
            
            // Score: 0% -> 0, 10% -> 100 (Linear)
            var volatilityScore = Math.Min(100, volatilityPercent * 1000); // 0.10 * 1000 = 100

            // 2. Trend (7-day change)
            // Assuming priceHistory is 30 days, we take last 7 days
            // If we only have 30 days data, we can just compare last price vs price 7 days ago
            decimal trendScore = 50; // Default neutral
            if (prices.Count >= 7)
            {
                var currentPrice = prices.Last();
                var price7DaysAgo = prices[prices.Count - 7];
                var change = (currentPrice - price7DaysAgo) / price7DaysAgo;
                
                // Score: -20% -> 100 (High Risk), +20% -> 0 (Low Risk)
                // Linear interpolation: 
                // Change = -0.2 -> Score = 100
                // Change = 0.2 -> Score = 0
                // Formula: Score = 50 - (change * 250)
                // Check: -0.2 * 250 = -50 => 50 - (-50) = 100
                // Check: 0.2 * 250 = 50 => 50 - 50 = 0
                trendScore = 50 - (change * 250);
                trendScore = Math.Max(0, Math.Min(100, trendScore));
            }

            // 3. Volume
            // Score: Current < Avg -> High Risk
            // Ratio = Current / Avg
            // Ratio < 0.5 -> 100
            // Ratio > 1.5 -> 0
            // Linear in between?
            // Let's say Ratio 1.0 -> 50
            // Formula: Score = 100 - (Ratio - 0.5) * 100
            // Check: Ratio 0.5 -> 100 - 0 = 100
            // Check: Ratio 1.5 -> 100 - 100 = 0
            decimal volumeScore = 50;
            if (averageVolume > 0)
            {
                var volumeRatio = currentVolume / averageVolume;
                volumeScore = 100 - ((volumeRatio - 0.5m) * 100);
                volumeScore = Math.Max(0, Math.Min(100, volumeScore));
            }

            // Composite Score
            // Weights: Volatility 40%, Trend 30%, Volume 30%
            var compositeScore = (volatilityScore * 0.4m) + (trendScore * 0.3m) + (volumeScore * 0.3m);

            return new RiskScoreResult
            {
                VolatilityScore = Math.Round(volatilityScore, 2),
                TrendScore = Math.Round(trendScore, 2),
                VolumeScore = Math.Round(volumeScore, 2),
                CompositeRiskScore = Math.Round(compositeScore, 2),
                PriceHistory = priceHistory
            };
        }
    }
}
