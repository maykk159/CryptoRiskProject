using CryptoRiskAnalysis.API.DTOs;
using CryptoRiskAnalysis.API.Interfaces;
using CryptoRiskAnalysis.API.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace CryptoRiskAnalysis.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RiskAnalysisController : ControllerBase
    {
        private readonly ICryptoDataService _cryptoDataService;
        private readonly IRiskEngine _riskEngine;
        private readonly ILogger<RiskAnalysisController> _logger;

        public RiskAnalysisController(
            ICryptoDataService cryptoDataService, 
            IRiskEngine riskEngine,
            ILogger<RiskAnalysisController> logger)
        {
            _cryptoDataService = cryptoDataService;
            _riskEngine = riskEngine;
            _logger = logger;
        }

        [HttpGet("{assetId}")]
        public async Task<ActionResult<ApiResponse<RiskAnalysisResponseDto>>> GetRiskAnalysis(
            string assetId, 
            [FromQuery] int days = 30)
        {
            _logger.LogInformation("Received risk analysis request for {AssetId} over {Days} days", assetId, days);

            // Validate days parameter - only allow 7, 30, or 90
            if (days != 7 && days != 30 && days != 90)
            {
                _logger.LogWarning("Invalid days parameter: {Days}. Defaulting to 30.", days);
                days = 30; // Default to 30 if invalid
            }

            // 1. Fetch ALL data in one call (optimized!)
            var (priceHistory, currentVolume, avgVolume) = await _cryptoDataService.GetAllMarketDataAsync(assetId, days);
            
            if (priceHistory == null || !priceHistory.Any())
            {
                _logger.LogWarning("No data found for asset: {AssetId}", assetId);
                return NotFound(new ApiResponse<RiskAnalysisResponseDto>($"No data found for asset: {assetId}"));
            }

            // 2. Calculate Risk (100% local - no API calls!)
            var riskResult = _riskEngine.CalculateRisk(priceHistory, currentVolume, avgVolume);

            // 3. Map to DTO
            var responseDto = new RiskAnalysisResponseDto
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

            _logger.LogInformation("Successfully calculated risk for {AssetId}: Score {Score}", assetId, riskResult.CompositeRiskScore);

            return Ok(new ApiResponse<RiskAnalysisResponseDto>(responseDto));
        }
    }
}
