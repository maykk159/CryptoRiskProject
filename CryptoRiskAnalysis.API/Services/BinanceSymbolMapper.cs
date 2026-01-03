namespace CryptoRiskAnalysis.API.Services
{
    /// <summary>
    /// Maps CoinGecko asset IDs to Binance trading symbols
    /// Identifies which assets can use Binance API vs need CoinGecko fallback
    /// </summary>
    public static class BinanceSymbolMapper
    {
        public static readonly Dictionary<string, string?> SymbolMap = new()
        {
            // ✅ Top cryptocurrencies - Available on Binance with high liquidity
            { "bitcoin", "BTCUSDT" },
            { "ethereum", "ETHUSDT" },
            { "ripple", "XRPUSDT" },
            { "binancecoin", "BNBUSDT" },
            { "solana", "SOLUSDT" },
            { "tron", "TRXUSDT" },
            { "dogecoin", "DOGEUSDT" },
            { "cardano", "ADAUSDT" },
            { "avalanche-2", "AVAXUSDT" },
            { "chainlink", "LINKUSDT" },
            { "shiba-inu", "SHIBUSDT" },
            { "bitcoin-cash", "BCHUSDT" },
            { "stellar", "XLMUSDT" },
            { "polkadot", "DOTUSDT" },
            { "litecoin", "LTCUSDT" },
            { "uniswap", "UNIUSDT" },
            // ⚠️ WBTC often has low liquidity or different pairs on Binance, safe to use CoinGecko
            { "wrapped-bitcoin", null }, 
            { "dai", "DAIUSDT" },
            
            // ⚠️ Stablecoins - Low liquidity on Binance, use CoinGecko instead
            { "tether", null },      // USDT is the quote currency, not a tradable pair
            { "usd-coin", null },    // USDC has low liquidity on Binance spot
        };
        
        /// <summary>
        /// Checks if an asset is available on Binance with good liquidity
        /// </summary>
        public static bool IsAvailableOnBinance(string coinGeckoId)
        {
            return SymbolMap.TryGetValue(coinGeckoId, out var symbol) && symbol != null;
        }
        
        /// <summary>
        /// Gets the Binance symbol for a CoinGecko asset ID
        /// </summary>
        public static string? GetBinanceSymbol(string coinGeckoId)
        {
            return SymbolMap.TryGetValue(coinGeckoId, out var symbol) ? symbol : null;
        }
    }
}
