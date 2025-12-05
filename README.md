# 📊 Crypto Risk Analysis

Full-stack cryptocurrency risk analysis application with real-time data from CoinGecko API.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![React](https://img.shields.io/badge/React-19.2-blue.svg)
![TypeScript](https://img.shields.io/badge/TypeScript-5.9-blue.svg)

## Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [npm 9+](https://www.npmjs.com/)

### Installation & Run

```powershell
# Clone the repository
git clone https://github.com/maykk159/CryptoRiskProject.git
cd CryptoRiskProject

# Backend
cd CryptoRiskAnalysis.API
dotnet restore
dotnet run --project CryptoRiskAnalysis.API/CryptoRiskAnalysis.API.csproj
# Backend will run on http://localhost:5058

# Frontend (in a new terminal)
cd client
npm install
npm run dev
# Frontend will run on http://localhost:5173
```

Open [http://localhost:5173](http://localhost:5173) in your browser.

## Features

- **Real-time Risk Analysis** - Live cryptocurrency risk scoring
- **Advanced Financial Metrics** - Industry-standard risk calculations
  - Log returns-based volatility
  - Downside risk (negative returns only)
  - Maximum drawdown tracking
  - Sharpe ratio (risk-adjusted returns)
  - Value at Risk (VaR 95%)
  - Annualized volatility
- **Adaptive Algorithms** - Context-aware risk scoring with weighted factors
- **Smart Caching** - 60-second in-memory cache to avoid API rate limits
- **Responsive UI** - Beautiful dark-mode dashboard with Tailwind CSS
- **20 Cryptocurrencies** - Bitcoin, Ethereum, Tether, XRP, BNB, Solana, USDC, TRON, Dogecoin, Cardano, Avalanche, Chainlink, Shiba Inu, Bitcoin Cash, Stellar, Polkadot, Litecoin, Uniswap, Wrapped Bitcoin, Dai

## Architecture

```
CryptoRiskAnalysis/
├── CryptoRiskAnalysis.API/    # .NET 8 Backend
│   ├── Controllers/            # API endpoints
│   ├── Services/              # Business logic
│   ├── Models/                # Data models
│   ├── DTOs/                  # Data transfer objects
│   └── Interfaces/            # Abstractions
│
└── client/                    # React Frontend
    ├── src/
    │   ├── components/        # UI components
    │   ├── services/          # API integration
    │   └── types/             # TypeScript definitions
    └── public/
```

## Tech Stack

### Backend
- **.NET 8.0** - Web API
- **C# 12** - Language
- **ASP.NET Core** - Framework
- **IMemoryCache** - Caching
- **HttpClient** - API calls

### Frontend
- **React 19.2** - UI framework
- **TypeScript 5.9** - Type safety
- **Vite 7.2** - Build tool
- **Tailwind CSS 3** - Styling
- **Axios 1.13** - HTTP client
- **Recharts 3.5** - Charts (optional)

### External APIs
- **CoinGecko API** - Cryptocurrency data

## Risk Calculation

### Core Risk Scores (0-100 scale)

1. **Volatility Score** (40% weight)
   - **Log returns** for mathematical accuracy
   - Annualized standard deviation with Bessel's correction (N-1)
   - Industry-standard financial metric
   - Formula: `σ_annual = σ_daily × √365`

2. **Trend Score** (30% weight)
   - 7-day vs 30-day momentum analysis
   - Extreme movement detection (both directions = risk)
   - Absolute value approach
   - Detects pump & dump patterns

3. **Volume Score** (30% weight)
   - Context-aware volume analysis
   - Price-volume divergence detection
   - Liquidity risk assessment
   - Panic selling and weak rally identification

4. **Composite Score**
   - Adaptive weighting based on market conditions
   - Risk amplification when multiple factors align
   - Risk dampening for stable conditions

### Advanced Financial Metrics

5. **Downside Risk**
   - Volatility of negative returns only
   - Focuses on loss scenarios
   - Formula: `σ_downside = √(Σ(min(r_i, 0))² / (n-1))`

6. **Maximum Drawdown**
   - Largest peak-to-trough decline (%)
   - Key metric for worst-case scenarios
   - Formula: `MDD = max(1 - P_t / max(P_0...P_t))`

7. **Sharpe Ratio**
   - Risk-adjusted return metric (annualized)
   - Higher values indicate better risk/return profile
   - Formula: `Sharpe = (μ - r_f) / σ × √365`
   - Uses 0% risk-free rate for crypto

8. **Value at Risk (VaR 95%)**
   - Worst-case loss at 95% confidence
   - 5th percentile of return distribution
   - Annualized percentage format

9. **Annualized Volatility**
   - Raw volatility as percentage
   - Standard deviation × √365 × 100

## Performance

- **API Optimization**: 66% fewer API calls (3 → 1 per request)
- **Cache Hit Rate**: 80-90% with 60-second TTL
- **Response Time**: < 200ms (cached), < 2s (API call)
- **Calculation Accuracy**: ~85% with production-grade formulas
- **Metric Coverage**: 9 comprehensive risk indicators

## Documentation

- [Backend Setup](./CryptoRiskAnalysis.API/README.md)
- [Frontend Setup](./client/README.md)
- [Risk Formula Analysis](./docs/risk-formulas.md)
- [Performance Optimization](./docs/optimization.md)

## Environment Variables

No environment variables required! Uses CoinGecko's free public API.

## Known Issues

- **CoinGecko Rate Limits**: Free tier has ~10-50 calls/minute. App uses caching and retry logic to mitigate.
- **CORS**: Backend must run on port 5058, frontend on 5173

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

MIT License - see [LICENSE](LICENSE) file for details

## Author

**Mayıs Kuru**
- GitHub: [@maykk159](https://github.com/maykk159)

## Acknowledgments

- [CoinGecko](https://www.coingecko.com/) for free cryptocurrency API
- Clean Architecture principles
- Financial risk calculation standards

## Support

For issues and questions:
- Open an [Issue](https://github.com/maykk159/CryptoRiskProject/issues)
- Check [Discussions](https://github.com/maykk159/CryptoRiskProject/discussions)

---

⭐ Star this repo if you find it helpful!
