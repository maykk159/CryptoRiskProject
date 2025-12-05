using CryptoRiskAnalysis.API.DTOs;
using CryptoRiskAnalysis.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CryptoRiskAnalysis.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RiskAnalysisController : ControllerBase
    {
        private readonly ICryptoDataService _cryptoDataService;
        private readonly IRiskEngine _riskEngine;

        public RiskAnalysisController(ICryptoDataService cryptoDataService, IRiskEngine riskEngine)
        {
            _cryptoDataService = cryptoDataService;
            _riskEngine = riskEngine;
        }

        [HttpGet("{assetId}")]
        public async Task<ActionResult<RiskAnalysisResponseDto>> GetRiskAnalysis(
            string assetId, 
            [FromQuery] int days = 30)
        {
            // Validate days parameter - only allow 7, 30, or 90
            if (days != 7 && days != 30 && days != 90)
            {
                days = 30; // Default to 30 if invalid
            }

            // 1. Fetch ALL data in one call (optimized!)
            var (priceHistory, currentVolume, avgVolume) = await _cryptoDataService.GetAllMarketDataAsync(assetId, days);
            
            if (priceHistory == null || !priceHistory.Any())
            {
                return NotFound($"No data found for asset: {assetId}");
            }

            // 2. Calculate Risk (100% local - no API calls!)
            var riskResult = _riskEngine.CalculateRisk(priceHistory, currentVolume, avgVolume);

            // 3. Map to DTO
            var response = new RiskAnalysisResponseDto
            {
                AssetId = assetId,
                CompositeRiskScore = riskResult.CompositeRiskScore,
                VolatilityScore = riskResult.VolatilityScore,
                TrendScore = riskResult.TrendScore,
                VolumeScore = riskResult.VolumeScore,
                DownsideRisk = riskResult.DownsideRisk,
                MaxDrawdown = riskResult.MaxDrawdown,
                SharpeRatio = riskResult.SharpeRatio,
                ValueAtRisk95 = riskResult.ValueAtRisk95,
                AnnualizedVolatility = riskResult.AnnualizedVolatility,
                PriceHistory = riskResult.PriceHistory
            };

            return Ok(response);
        }
    }
}
