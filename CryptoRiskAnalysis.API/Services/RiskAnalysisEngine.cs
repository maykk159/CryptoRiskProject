using CryptoRiskAnalysis.API.Interfaces;
using CryptoRiskAnalysis.API.Models;

namespace CryptoRiskAnalysis.API.Services
{
    public class RiskAnalysisEngine : IRiskEngine
    {
        // Risk calculation constants
        private const decimal EXTREME_MOMENTUM_THRESHOLD = 0.30m;  // 30% movement
        private const decimal SIGNIFICANT_MOMENTUM_THRESHOLD = 0.15m;  // 15% movement
        private const decimal MODERATE_MOMENTUM_THRESHOLD = 0.05m;  // 5% movement
        
        private const decimal EXTREME_MOMENTUM_BASE_SCORE = 80m;
        private const decimal SIGNIFICANT_MOMENTUM_BASE_SCORE = 50m;
        private const decimal MODERATE_MOMENTUM_BASE_SCORE = 20m;
        
        private const decimal EXTREME_MOMENTUM_MULTIPLIER = 200m;
        private const decimal SIGNIFICANT_MOMENTUM_MULTIPLIER = 200m;
        private const decimal MODERATE_MOMENTUM_MULTIPLIER = 300m;
        private const decimal STABLE_MOMENTUM_MULTIPLIER = 400m;
        
        private const decimal VOLATILITY_MODERATE_THRESHOLD = 0.5m;  // 50% annual volatility
        private const decimal VOLATILITY_HIGH_THRESHOLD = 1.0m;  // 100% annual volatility
        
        private const decimal HIGH_RISK_THRESHOLD = 70m;
        private const decimal LOW_RISK_THRESHOLD = 30m;
        
        private const int CRYPTO_TRADING_DAYS_PER_YEAR = 365;  // Crypto trades 24/7
        private const int SHORT_TERM_DAYS = 7;
        private const int MINIMUM_DATA_POINTS = 7;
        public RiskScoreResult CalculateRisk(List<PriceData> priceHistory, decimal currentVolume, decimal averageVolume)
        {
            if (priceHistory == null || !priceHistory.Any())
            {
                return new RiskScoreResult();
            }

            var prices = priceHistory.Select(p => p.Price).ToList();
            
            // Calculate log returns (more mathematically sound)
            var returns = CalculateLogReturns(prices);
            
            // ✅ IMPROVED: Financial-grade risk calculations
            var volatilityScore = CalculateVolatilityScore(returns);
            var trendScore = CalculateTrendScore(prices);
            var volumeScore = CalculateVolumeScore(prices, currentVolume, averageVolume);
            var compositeScore = CalculateCompositeScore(volatilityScore, trendScore, volumeScore);
            
            // ✨ NEW: Advanced financial metrics
            var downsideRisk = CalculateDownsideRisk(returns);
            var maxDrawdown = CalculateMaximumDrawdown(prices);
            var sharpeRatio = CalculateSharpeRatio(returns);
            var valueAtRisk95 = CalculateValueAtRisk95(returns);
            var annualizedVol = CalculateAnnualizedVolatility(returns);

            return new RiskScoreResult
            {
                VolatilityScore = Math.Round(volatilityScore, 2),
                TrendScore = Math.Round(trendScore, 2),
                VolumeScore = Math.Round(volumeScore, 2),
                CompositeRiskScore = Math.Round(compositeScore, 2),
                DownsideRisk = Math.Round(downsideRisk, 2),
                MaxDrawdown = Math.Round(maxDrawdown, 2),
                SharpeRatio = Math.Round(sharpeRatio, 2),
                ValueAtRisk95 = Math.Round(valueAtRisk95, 2),
                AnnualizedVolatility = Math.Round(annualizedVol, 2),
                PriceHistory = priceHistory
            };
        }

        /// <summary>
        /// Calculate log returns from price series
        /// Log returns are more mathematically sound for financial calculations
        /// </summary>
        private List<double> CalculateLogReturns(List<decimal> prices)
        {
            var returns = new List<double>();
            for (int i = 1; i < prices.Count; i++)
            {
                if (prices[i - 1] == 0 || prices[i] == 0) continue;
                // Log return: ln(P_t / P_t-1)
                var logReturn = Math.Log((double)(prices[i] / prices[i - 1]));
                returns.Add(logReturn);
            }
            return returns;
        }

        /// <summary>
        /// Calculate volatility using log returns and annualized standard deviation
        /// This is the industry standard for financial volatility measurement
        /// </summary>
        private decimal CalculateVolatilityScore(List<double> returns)
        {
            if (returns.Count < 2) return 50m; // Not enough data

            // Sample standard deviation (using N-1 for Bessel's correction)
            var mean = returns.Average();
            var sumOfSquares = returns.Sum(r => Math.Pow(r - mean, 2));
            var variance = sumOfSquares / (returns.Count - 1); // N-1 correction
            var dailyStdDev = Math.Sqrt(variance);

            // Annualize the volatility (standard financial practice)
            // Daily volatility * sqrt(365) for crypto (24/7 trading)
            var annualizedVolatility = dailyStdDev * Math.Sqrt(CRYPTO_TRADING_DAYS_PER_YEAR);

            // Convert to 0-100 scale
            // Crypto typical annual volatility: 50% = moderate, 100% = high, 200%+ = extreme
            // Score mapping: 0% -> 0, 50% -> 50, 100% -> 75, 200%+ -> 100
            decimal score;
            if ((decimal)annualizedVolatility < VOLATILITY_MODERATE_THRESHOLD) // < 50% annual vol
                score = (decimal)annualizedVolatility * 100; // 0-50
            else if ((decimal)annualizedVolatility < VOLATILITY_HIGH_THRESHOLD) // 50-100% annual vol
                score = 50 + ((decimal)annualizedVolatility - VOLATILITY_MODERATE_THRESHOLD) * 50; // 50-75
            else // > 100% annual vol
                score = Math.Min(100, 75 + ((decimal)annualizedVolatility - VOLATILITY_HIGH_THRESHOLD) * 25); // 75-100

            return score;
        }

        /// <summary>
        /// Calculate trend risk using momentum analysis with extreme movement detection
        /// Extreme movements in BOTH directions indicate high risk
        /// </summary>
        private decimal CalculateTrendScore(List<decimal> prices)
        {
            if (prices.Count < MINIMUM_DATA_POINTS) return 50m; // Not enough data for trend

            // Calculate short-term (7-day) vs long-term (30-day) momentum
            var recent7Days = prices.Skip(Math.Max(0, prices.Count - SHORT_TERM_DAYS)).ToList();
            var avg7Day = recent7Days.Average();
            var avg30Day = prices.Average();

            // Momentum: short-term avg vs long-term avg
            var momentum = (avg7Day - avg30Day) / avg30Day;
            var absMomentum = Math.Abs(momentum);

            // CRITICAL: Extreme movements (both up and down) indicate risk!
            // Pump & dump, panic selling, volatility spikes all = high risk
            decimal score;

            if (absMomentum > EXTREME_MOMENTUM_THRESHOLD)
            {
                // Very high risk - potential pump/dump or crash
                score = Math.Min(100, EXTREME_MOMENTUM_BASE_SCORE + (absMomentum - EXTREME_MOMENTUM_THRESHOLD) * EXTREME_MOMENTUM_MULTIPLIER);
            }
            else if (absMomentum > SIGNIFICANT_MOMENTUM_THRESHOLD)
            {
                // High risk - strong momentum
                score = SIGNIFICANT_MOMENTUM_BASE_SCORE + (absMomentum - SIGNIFICANT_MOMENTUM_THRESHOLD) * SIGNIFICANT_MOMENTUM_MULTIPLIER;
            }
            else if (absMomentum > MODERATE_MOMENTUM_THRESHOLD)
            {
                // Moderate risk - normal trend
                score = MODERATE_MOMENTUM_BASE_SCORE + (absMomentum - MODERATE_MOMENTUM_THRESHOLD) * MODERATE_MOMENTUM_MULTIPLIER;
            }
            else
            {
                // Low risk - stable price
                score = absMomentum * STABLE_MOMENTUM_MULTIPLIER;
            }

            return score;
        }

        /// <summary>
        /// Calculate volume-based risk with price context
        /// Analyzes volume patterns in relation to price movements
        /// </summary>
        private decimal CalculateVolumeScore(List<decimal> prices, decimal currentVolume, decimal averageVolume)
        {
            if (averageVolume == 0) return 50m; // No volume data

            var volumeRatio = currentVolume / averageVolume;
            
            // Calculate recent price change for context
            decimal priceChange = 0;
            if (prices.Count >= 7)
            {
                var recent = prices.Last();
                var weekAgo = prices[prices.Count - 7];
                priceChange = (recent - weekAgo) / weekAgo;
            }

            decimal score = 40m; // Base score

            // Context-aware volume analysis
            
            // 1. Selling pressure: Declining price + High volume = PANIC
            if (priceChange < -0.05m && volumeRatio > 1.5m)
            {
                // Strong selling pressure
                score = Math.Min(100, 70 + (volumeRatio - 1.5m) * 20);
            }
            // 2. Weak rally: Rising price + Low volume = UNSUSTAINABLE
            else if (priceChange > 0.05m && volumeRatio < 0.5m)
            {
                // Weak momentum, likely reversal
                score = Math.Min(100, 60 + (0.5m - volumeRatio) * 60);
            }
            // 3. Very low liquidity = High slippage risk
            else if (volumeRatio < 0.3m)
            {
                // Low liquidity regardless of price
                score = Math.Min(100, 65 + (0.3m - volumeRatio) * 100);
            }
            // 4. Volume spike = Potential volatility
            else if (volumeRatio > 3.0m)
            {
                // Unusual activity, potential manipulation or news
                score = Math.Min(100, 55 + (volumeRatio - 3.0m) * 10);
            }
            // 5. High volume + rising price = Healthy but watch
            else if (priceChange > 0.10m && volumeRatio > 1.5m)
            {
                // Strong rally, but overheating risk
                score = 45 + (volumeRatio - 1.5m) * 15;
            }
            // 6. Normal range
            else
            {
                // Stable volume around average
                score = 30 + Math.Abs(volumeRatio - 1.0m) * 20;
            }

            return Math.Max(0, Math.Min(100, score));
        }

        /// <summary>
        /// Calculate composite risk score with adaptive weighting and risk amplification
        /// </summary>
        private decimal CalculateCompositeScore(decimal volatilityScore, decimal trendScore, decimal volumeScore)
        {
            // Default weights
            decimal volWeight = 0.40m;
            decimal trendWeight = 0.30m;
            decimal volumeWeight = 0.30m;

            // ✅ ADAPTIVE WEIGHTING based on market conditions
            
            // When volatility is extreme, it dominates
            if (volatilityScore > HIGH_RISK_THRESHOLD)
            {
                volWeight = 0.50m;
                trendWeight = 0.25m;
                volumeWeight = 0.25m;
            }
            // When trend is extreme, it's critical
            else if (trendScore > HIGH_RISK_THRESHOLD + 10)
            {
                volWeight = 0.30m;
                trendWeight = 0.45m;
                volumeWeight = 0.25m;
            }
            // When volume shows panic/manipulation
            else if (volumeScore > HIGH_RISK_THRESHOLD)
            {
                volWeight = 0.35m;
                trendWeight = 0.25m;
                volumeWeight = 0.40m;
            }

            // Weighted average
            var composite = (volatilityScore * volWeight) 
                          + (trendScore * trendWeight) 
                          + (volumeScore * volumeWeight);

            // ✅ RISK AMPLIFICATION: Multiple high risks compound
            
            // All three high = systemic risk
            if (volatilityScore > HIGH_RISK_THRESHOLD && trendScore > HIGH_RISK_THRESHOLD && volumeScore > HIGH_RISK_THRESHOLD)
            {
                composite = Math.Min(100, composite * 1.20m); // 20% penalty
            }
            // Two high = significant risk
            else if ((volatilityScore > HIGH_RISK_THRESHOLD && trendScore > HIGH_RISK_THRESHOLD) ||
                     (volatilityScore > HIGH_RISK_THRESHOLD && volumeScore > HIGH_RISK_THRESHOLD) ||
                     (trendScore > HIGH_RISK_THRESHOLD && volumeScore > HIGH_RISK_THRESHOLD))
            {
                composite = Math.Min(100, composite * 1.10m); // 10% penalty
            }

            // ✅ RISK DAMPENING: Low across the board = extra safe
            if (volatilityScore < LOW_RISK_THRESHOLD && trendScore < LOW_RISK_THRESHOLD && volumeScore < LOW_RISK_THRESHOLD)
            {
                composite = composite * 0.90m; // 10% bonus (reduce risk score)
            }

            return Math.Max(0, Math.Min(100, composite));
        }

        /// <summary>
        /// Calculate downside risk - volatility of negative returns only
        /// Focuses on downside movements, ignoring positive returns
        /// </summary>
        private decimal CalculateDownsideRisk(List<double> returns)
        {
            if (returns.Count < 2) return 0m;

            var downsideReturns = returns.Where(r => r < 0).ToList();
            if (downsideReturns.Count < 2) return 0m;

            var mean = downsideReturns.Average();
            var sumOfSquares = downsideReturns.Sum(r => Math.Pow(r - mean, 2));
            var variance = sumOfSquares / (downsideReturns.Count - 1);
            var downsideStdDev = Math.Sqrt(variance);

            // Annualize and convert to percentage
            var annualizedDownside = downsideStdDev * Math.Sqrt(CRYPTO_TRADING_DAYS_PER_YEAR) * 100;
            return (decimal)annualizedDownside;
        }

        /// <summary>
        /// Calculate maximum drawdown - largest peak-to-trough decline
        /// Key metric for understanding worst-case scenario
        /// </summary>
        private decimal CalculateMaximumDrawdown(List<decimal> prices)
        {
            if (prices.Count < 2) return 0m;

            decimal maxDrawdown = 0m;
            decimal peak = prices[0];

            foreach (var price in prices)
            {
                if (price > peak)
                    peak = price;

                var drawdown = (peak - price) / peak * 100; // Percentage decline
                if (drawdown > maxDrawdown)
                    maxDrawdown = drawdown;
            }

            return maxDrawdown;
        }

        /// <summary>
        /// Calculate Sharpe ratio - risk-adjusted return metric
        /// Higher is better (more return per unit of risk)
        /// </summary>
        private decimal CalculateSharpeRatio(List<double> returns)
        {
            if (returns.Count < 2) return 0m;

            var mean = returns.Average();
            var sumOfSquares = returns.Sum(r => Math.Pow(r - mean, 2));
            var variance = sumOfSquares / (returns.Count - 1);
            var stdDev = Math.Sqrt(variance);

            if (stdDev == 0) return 0m;

            // Sharpe = (Mean Return - Risk Free Rate) / StdDev
            // Using 0% risk-free rate for crypto (no true risk-free baseline)
            var sharpe = mean / stdDev;

            // Annualize the Sharpe ratio
            var annualizedSharpe = sharpe * Math.Sqrt(CRYPTO_TRADING_DAYS_PER_YEAR);

            return (decimal)annualizedSharpe;
        }

        /// <summary>
        /// Calculate Value at Risk at 95% confidence level
        /// Represents worst-case loss in 95% of scenarios
        /// </summary>
        private decimal CalculateValueAtRisk95(List<double> returns)
        {
            if (returns.Count < 20) return 0m; // Need sufficient data for percentiles

            var sortedReturns = returns.OrderBy(r => r).ToList();
            
            // 5th percentile (95% confidence - worst 5%)
            var index = (int)(sortedReturns.Count * 0.05);
            var var95 = sortedReturns[index];

            // Annualize and convert to percentage (make positive for clarity)
            var annualizedVaR = Math.Abs(var95 * Math.Sqrt(CRYPTO_TRADING_DAYS_PER_YEAR) * 100);

            return (decimal)annualizedVaR;
        }

        /// <summary>
        /// Calculate raw annualized volatility as a percentage
        /// </summary>
        private decimal CalculateAnnualizedVolatility(List<double> returns)
        {
            if (returns.Count < 2) return 0m;

            var mean = returns.Average();
            var sumOfSquares = returns.Sum(r => Math.Pow(r - mean, 2));
            var variance = sumOfSquares / (returns.Count - 1);
            var dailyStdDev = Math.Sqrt(variance);

            // Annualize and convert to percentage
            var annualizedVolatility = dailyStdDev * Math.Sqrt(CRYPTO_TRADING_DAYS_PER_YEAR) * 100;

            return (decimal)annualizedVolatility;
        }
    }
}
