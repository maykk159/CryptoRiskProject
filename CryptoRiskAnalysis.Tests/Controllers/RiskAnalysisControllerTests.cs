using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CryptoRiskAnalysis.API.Controllers;
using CryptoRiskAnalysis.API.Interfaces;
using CryptoRiskAnalysis.API.Models;
using CryptoRiskAnalysis.API.DTOs;
using CryptoRiskAnalysis.API.Wrappers;

namespace CryptoRiskAnalysis.Tests.Controllers
{
    public class RiskAnalysisControllerTests
    {
        private readonly Mock<ICryptoDataService> _mockCryptoService;
        private readonly Mock<IRiskEngine> _mockRiskEngine;
        private readonly Mock<ILogger<RiskAnalysisController>> _mockLogger;
        private readonly RiskAnalysisController _controller;

        public RiskAnalysisControllerTests()
        {
            _mockCryptoService = new Mock<ICryptoDataService>();
            _mockRiskEngine = new Mock<IRiskEngine>();
            _mockLogger = new Mock<ILogger<RiskAnalysisController>>();
            _controller = new RiskAnalysisController(
                _mockCryptoService.Object,
                _mockRiskEngine.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetRiskAnalysis_ValidAsset_ReturnsOkResult()
        {
            // Arrange
            var assetId = "bitcoin";
            var priceHistory = new List<PriceData>
            {
                new PriceData { Timestamp = 1000, Price = 100m },
                new PriceData { Timestamp = 2000, Price = 105m }
            };

            var riskResult = new RiskScoreResult
            {
                VolatilityScore = 50m,
                TrendScore = 45m,
                VolumeScore = 55m,
                CompositeRiskScore = 50m,
                DownsideRisk = 10m,
                MaxDrawdown = 5m,
                SharpeRatio = 1.5m,
                ValueAtRisk95 = 3m,
                AnnualizedVolatility = 20m,
                PriceHistory = priceHistory
            };

            _mockCryptoService.Setup(s => s.GetAllMarketDataAsync(assetId, 30))
                .ReturnsAsync((priceHistory, 1000m, 900m));
            _mockRiskEngine.Setup(e => e.CalculateRisk(It.IsAny<List<PriceData>>(), 1000m, 900m))
                .Returns(riskResult);

            // Act
            var result =await _controller.GetRiskAnalysis(assetId, 30);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<RiskAnalysisResponseDto>>(okResult.Value);
            Assert.True(response.Succeeded);
            Assert.NotNull(response.Data);
            Assert.Equal(50m, response.Data.CompositeRiskScore);
        }

        [Fact]
        public async Task GetRiskAnalysis_InvalidDays_DefaultsTo30()
        {
            // Arrange
            var assetId = "bitcoin";
            var priceHistory = new List<PriceData>
            {
                new PriceData { Timestamp = 1000, Price = 100m }
            };

            _mockCryptoService.Setup(s => s.GetAllMarketDataAsync(assetId, 30))
                .ReturnsAsync((priceHistory, 1000m, 900m));
            _mockRiskEngine.Setup(e => e.CalculateRisk(It.IsAny<List<PriceData>>(), 1000m, 900m))
                .Returns(new RiskScoreResult { CompositeRiskScore = 50m, PriceHistory = priceHistory });

            // Act - Pass invalid days (e.g., 0)
            var result = await _controller.GetRiskAnalysis(assetId, 0);

            // Assert - Should default to 30
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockCryptoService.Verify(s => s.GetAllMarketDataAsync(assetId, 30), Times.Once);
        }

        [Fact]
        public async Task GetRiskAnalysis_NoDataFound_ReturnsNotFound()
        {
            // Arrange
            var assetId = "invalid_coin";
            _mockCryptoService.Setup(s => s.GetAllMarketDataAsync(assetId, 30))
                .ReturnsAsync((new List<PriceData>(), 0m, 0m));

            // Act
            var result = await _controller.GetRiskAnalysis(assetId, 30);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<ApiResponse<RiskAnalysisResponseDto>>(notFoundResult.Value);
            Assert.False(response.Succeeded);
            Assert.Contains("No data found", response.Message);
        }

        [Fact]
        public async Task GetRiskAnalysis_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var assetId = "bitcoin";
            _mockCryptoService.Setup(s => s.GetAllMarketDataAsync(assetId, 30))
                .ThrowsAsync(new Exception("API failure"));

            // Act
            var result = await _controller.GetRiskAnalysis(assetId, 30);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetRiskAnalysis_FiltersHistoryToRequestedDays()
        {
            // Arrange - Service returns 30 days, but user requested 7
            var assetId = "bitcoin";
            var priceHistory = new List<PriceData>();
            for (int i = 0; i < 30; i++)
            {
                priceHistory.Add(new PriceData
                {
                    Timestamp = DateTimeOffset.UtcNow.AddDays(-30 + i).ToUnixTimeMilliseconds(),
                    Price = 100m + i
                });
            }

            var riskResult = new RiskScoreResult
            {
                CompositeRiskScore = 50m,
                PriceHistory = priceHistory
            };

            _mockCryptoService.Setup(s => s.GetAllMarketDataAsync(assetId, 30))
                .ReturnsAsync((priceHistory, 1000m, 900m));
            _mockRiskEngine.Setup(e => e.CalculateRisk(It.IsAny<List<PriceData>>(), 1000m, 900m))
                .Returns(riskResult);

            // Act - Request 7 days
            var result = await _controller.GetRiskAnalysis(assetId, 7);

            // Assert - Response should only contain 7 days
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<RiskAnalysisResponseDto>>(okResult.Value);
            Assert.Equal(7, response.Data.PriceHistory.Count);
        }
    }
}
