using CryptoRiskAnalysis.API.Interfaces;
using CryptoRiskAnalysis.API.Services;

namespace CryptoRiskAnalysis.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Add Memory Cache
            services.AddMemoryCache();

            // Register API Services with HttpClient
            services.AddHttpClient<CoinGeckoService>();
            services.AddHttpClient<BinanceSpotService>();
            services.AddHttpClient<HybridCryptoDataService>();

            // Register HybridCryptoDataService as the implementation of ICryptoDataService
            services.AddScoped<ICryptoDataService>(sp => sp.GetRequiredService<HybridCryptoDataService>());

            // Register Risk Engine
            services.AddScoped<IRiskEngine, RiskAnalysisEngine>();

            return services;
        }

        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    builder => builder.WithOrigins("http://localhost:5173", "http://localhost:5174")
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });

            return services;
        }
    }
}
