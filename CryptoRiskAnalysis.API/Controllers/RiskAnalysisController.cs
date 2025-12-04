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
        public async Task<ActionResult<RiskAnalysisResponseDto>> GetRiskAnalysis(string assetId)
        {
            // 1. Fetch Data
            var priceHistory = await _cryptoDataService.GetHistoricalPriceDataAsync(assetId, 30);
            if (priceHistory == null || !priceHistory.Any())
            {
                return NotFound($"No data found for asset: {assetId}");
            }

            var currentVolume = await _cryptoDataService.GetCurrentVolumeAsync(assetId);
            var avgVolume = await _cryptoDataService.GetAverageVolumeAsync(assetId, 30);

            // 2. Calculate Risk
            var riskResult = _riskEngine.CalculateRisk(priceHistory, currentVolume, avgVolume);

            // 3. Map to DTO
            var response = new RiskAnalysisResponseDto
            {
                AssetId = assetId,
                CompositeRiskScore = riskResult.CompositeRiskScore,
                VolatilityScore = riskResult.VolatilityScore,
                TrendScore = riskResult.TrendScore,
                VolumeScore = riskResult.VolumeScore,
                PriceHistory = riskResult.PriceHistory
            };

            return Ok(response);
        }
    }
}
