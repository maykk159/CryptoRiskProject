import React from 'react';

interface AdvancedMetricsProps {
    data: {
        downsideRisk: number;
        maxDrawdown: number;
        sharpeRatio: number;
        valueAtRisk95: number;
        annualizedVolatility: number;
    };
}

export const AdvancedMetrics: React.FC<AdvancedMetricsProps> = ({ data }) => {
    return (
        <div className="bg-gray-800 rounded-xl p-6 shadow-lg border border-gray-700">
            <h2 className="text-xl font-bold text-white mb-6">Advanced Metrics</h2>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {/* Downside Risk */}
                <div className="bg-gray-900 p-4 rounded-lg" title="Volatility of negative returns only - measures downside risk">
                    <p className="text-gray-400 text-sm mb-1">Downside Risk</p>
                    <p className="text-white text-lg font-semibold">{data.downsideRisk.toFixed(2)}%</p>
                    <p className="text-gray-500 text-xs mt-1">Downside volatility only</p>
                </div>

                {/* Max Drawdown */}
                <div className="bg-gray-900 p-4 rounded-lg" title="Largest peak-to-trough decline in the period">
                    <p className="text-gray-400 text-sm mb-1">Max Drawdown</p>
                    <p className="text-red-400 text-lg font-semibold">-{data.maxDrawdown.toFixed(2)}%</p>
                    <p className="text-gray-500 text-xs mt-1">Worst-case decline</p>
                </div>

                {/* Sharpe Ratio */}
                <div className="bg-gray-900 p-4 rounded-lg" title="Risk-adjusted return metric - higher is better">
                    <p className="text-gray-400 text-sm mb-1">Sharpe Ratio</p>
                    <p className={`text-lg font-semibold ${data.sharpeRatio >= 1 ? 'text-green-400' :
                            data.sharpeRatio >= 0 ? 'text-yellow-400' :
                                'text-red-400'
                        }`}>
                        {data.sharpeRatio.toFixed(2)}
                    </p>
                    <p className="text-gray-500 text-xs mt-1">Risk-adjusted return</p>
                </div>

                {/* VaR 95% */}
                <div className="bg-gray-900 p-4 rounded-lg" title="95% confidence worst-case loss">
                    <p className="text-gray-400 text-sm mb-1">VaR (95%)</p>
                    <p className="text-orange-400 text-lg font-semibold">-{data.valueAtRisk95.toFixed(2)}%</p>
                    <p className="text-gray-500 text-xs mt-1">95% confidence loss</p>
                </div>

                {/* Annualized Volatility */}
                <div className="bg-gray-900 p-4 rounded-lg col-span-1 md:col-span-2" title="Standard deviation annualized">
                    <p className="text-gray-400 text-sm mb-1">Annualized Volatility</p>
                    <p className="text-purple-400 text-lg font-semibold">{data.annualizedVolatility.toFixed(2)}%</p>
                    <p className="text-gray-500 text-xs mt-1">Historical price volatility</p>
                </div>
            </div>
        </div>
    );
};
