# SUMMARY OF THE WORK CARRIED OUT DURING THE SEMESTER

> **Note to Student**: Ensure this document is formatted in **Times New Roman, 12-point font, justified alignment** before submission. Equations should be inserted using an Equation Editor.

During the first semester of the "Financial Risk Analysis Tool" capstone project, the primary objective was to establish a robust full-stack software architecture and implement the core risk analysis algorithms. The work carried out can be categorized into four main phases: System Architecture Design, Data Integration, Algorithmic Implementation, and User Interface Development.

**1. System Architecture and Backend Development**
The project was initialized using a **Clean Architecture** methodology to ensure separation of concerns and maintainability. The backend was developed using **.NET 8 Web API**, providing a strongly typed and high-performance foundation. The architecture logic was divided into three distinct layers:
*   **Services Layer**: Encapsulates business logic, including data fetching strategies and risk calculation engines.
*   **Controllers Layer**: Handles HTTP requests and response formatting, adhering to RESTful principles.
*   **Models Layer**: Defines Data Transfer Objects (DTOs) and domain entities.
Dependency Injection (DI) was implemented essentially to manage service lifecycles (`AddScoped`), facilitating modular testing and scalability.

**2. Hybrid Data Integration Strategy**
A significant portion of the semester was dedicated to solving the challenge of reliable real-time market data acquisition. A **Hybrid Crypto Data Service** was engineered to mitigate API rate limits and ensure data continuity:
*   **Primary Source**: The **Binance Spot API** was integrated to fetch OHLCV (Open, High, Low, Close, Volume) "Kline" specific data. This source allows for high-frequency requests, which are cached for **60 seconds** to optimize performance.
*   **Secondary Source**: A fallback mechanism using the **CoinGecko API** was implemented. If the primary source is unavailable, the system automatically switches to CoinGecko, caching responses for **180 seconds** to comply with stricter rate limits.
*   **Volume Analysis Correction**: To ensure statistical accuracy, the data fetching logic was refined to analyze the "last completed daily candle" rather than real-time incomplete data, eliminating false signals in volume trend analysis.

**3. Risk Analysis Engine Implementation**
The core functionality of the project, the `RiskAnalysisEngine`, was developed to quantify financial risk through a multi-dimensional heuristic algorithm. The engine computes a composite risk score (0-100) based on three fundamental metrics:
*   **Volatility Analysis**: Utilizes annualized standard deviation of log returns. An adaptive weighting mechanism was introduced where the weight of volatility in the final score increases from 40% to 50% during periods of extreme market instability ($>100\%$ annualized volatility).
*   **Trend and Momentum**: Analyzes short-term (7-day) versus long-term (30-day) price channel deviations.
*   **Liquidity Risk**: Evaluates volume depth to assess slippage risk.
Furthermore, a **Risk Amplification** logic was coded to penalize assets exhibiting simultaneous warning signs across multiple metrics, multiplying the risk score by a factor of 1.1x or 1.2x. Advanced financial metrics, including the **Sharpe Ratio**, **Maximum Drawdown**, and **Value at Risk (VaR 95%)**, were also integrated to provide a comprehensive risk profile.

**4. Frontend Development**
The client-side application was built using **React 19** and **TypeScript**, leveraging **Vite** for optimized build performance. A modern user interface was designed with **Tailwind CSS**, featuring dark-mode aesthetics suitable for financial trading environments. **Recharts** was employed to render dynamic visualization of risk scores and volume trends, allowing for immediate visual interpretation of the underlying data.

---

# MATERIALS AND METHODS USED IN THE PROJECT

> **Note to Student**: Ensure this section is formatted in **Times New Roman, 12-point font, justified alignment**. All references must be cited in the final document.

## Material List

The development of the project necessitated the following hardware and software resources:

*   **Hardware Components (with technical information)**:
    *   **Development Workstation**: x64-based PC architecture capable of supporting virtualization and heavy IDE usage.
    *   **Network Infrastructure**: High-speed broadband connection required for low-latency communication with external financial data providers (Binance/CoinGecko).

*   **Databases**:
    *   **In-Memory Cache**: **Microsoft.Extensions.Caching.Memory** was used for high-speed, temporary data storage to handle session-level market data without the latency overhead of a persistent database.
    *   **Serialization**: **System.Text.Json** was employed for high-performance parsing of JSON payloads from external APIs.

*   **Coding**:
    *   **Backend**: **C#** running on the **.NET 8** framework.
    *   **Frontend**: **TypeScript** (~5.9) and **React 19**.
    *   **Styling**: **CSS3** via **Tailwind CSS**.

*   **Package Programs**:
    *   **Visual Studio Code / Visual Studio 2022**: Primary IDEs.
    *   **Git**: Version control.
    *   **Postman**: API testing tool.

*   **Plugins**:
    *   **Backend**: `Swashbuckle.AspNetCore` (6.6.2).
    *   **Frontend**: `Axios` (^1.13), `Recharts` (^3.5).

*   **Auxiliary Tools, Equipment, or Code**:
    *   **Vite**: Frontend build tool and development server.
    *   **Middleware**: Custom `ExceptionHandlingMiddleware` for robust error management.

## Methods

This project applies a **Quantitative Computational Method** to assess financial risk, supported by specific software engineering patterns.

**1. Reliable Data Acquisition Method (Smart Routing Pattern)**
To ensure high availability of market data, a "Smart Routing" pattern was implemented within the `HybridCryptoDataService`. This method prioritizes high-performance endpoints while maintaining a fail-safe mechanism:
*   The system first queries the **Binance API**. Successful responses are serialized and cached (`IMemoryCache`) for **60 seconds**.
*   In the event of an exception or timeout, the system degrades gracefully to the **CoinGecko API**, extending the cache duration to **180 seconds** to preserve the limited request quota.

**2. Algorithmic Risk Assessment Method**
The risk evaluation methodology treats crypto assets as stochastic time series data. The `RiskAnalysisEngine` implements a heuristic algorithm defined by the following weighted function:

$$ RiskScore = \min(100, \alpha (w_{v} \cdot V + w_{t} \cdot T + w_{l} \cdot L)) $$

Where:
*   $V$, $T$, $L$ represent the normalized scores for Volatility, Trend, and Liquidity respectively.
*   $w$ represents the adaptive weights, which dynamically adjust based on market regime detection (e.g., $w_{v}$ increases during high-volatility regimes).
*   $\alpha$ represents the Risk Amplification factor (1.0, 1.1, or 1.2), triggered when multiple risk thresholds are breached simultaneously.

This method allows distinct risk factors to influence the final score proportionally to their severity, providing a nuanced risk assessment rather than a simple linear average.

**3. Software Design Methodology**
The project strictly follows **Clean Architecture** principles. By decoupling the Domain (Models) and Application (Services) layers from the Infrastructure (External APIs), the system remains agnostic to specific data providers. This modular approach allows for the individual testing and verification of the risk algorithms independent of the data source or user interface.
