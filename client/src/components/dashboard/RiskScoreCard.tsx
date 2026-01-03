import React from 'react';

interface RiskScoreCardProps {
    data: {
        compositeRiskScore: number;
        volatilityScore: number;
        trendScore: number;
        volumeScore: number;
    };
    asset?: {
        name: string;
        ticker: string;
        icon: string;
    };
}

export const RiskScoreCard: React.FC<RiskScoreCardProps> = ({ data, asset }) => {
    const getRiskLevel = (score: number) => {
        if (score < 30) return { text: 'Low Risk', color: 'text-green-400', bgColor: 'bg-green-400' };
        if (score < 70) return { text: 'Medium Risk', color: 'text-yellow-400', bgColor: 'bg-yellow-400' };
        return { text: 'High Risk', color: 'text-red-400', bgColor: 'bg-red-400' };
    };

    const riskLevel = getRiskLevel(data.compositeRiskScore);

    return (
        <div className="bg-gray-800 rounded-xl p-6 shadow-lg border border-gray-700">
            <div className="flex items-center gap-4 mb-6">
                {asset && (
                    <img
                        src={asset.icon}
                        alt={asset.name}
                        className="w-10 h-10 rounded-full"
                        onError={(e) => {
                            (e.target as HTMLImageElement).style.display = 'none'; // Hide if fails
                        }}
                    />
                )}
                <h2 className="text-xl font-bold text-white">
                    {asset ? asset.name : 'Risk Analysis'}
                    {asset && <span className="ml-2 text-gray-400 text-lg">({asset.ticker})</span>}
                </h2>
            </div>

            <div className="flex items-center justify-between mb-8 p-4 bg-gray-900 rounded-lg">
                <div>
                    <p className="text-gray-400 text-sm">Composite Risk Score</p>
                    <p className={`text-3xl font-bold ${riskLevel.color}`}>
                        {data.compositeRiskScore.toFixed(1)}
                    </p>
                </div>
                <div className={`text-lg font-semibold px-4 py-2 rounded-full ${riskLevel.color}`}>
                    {riskLevel.text}
                </div>
            </div>

            <div className="space-y-6">
                {/* Volatility Score */}
                <div className="mb-4">
                    <div className="flex justify-between mb-1">
                        <span className="text-sm font-medium text-gray-300">Volatility Risk</span>
                        <span className="text-sm font-medium text-white">{data.volatilityScore.toFixed(1)}</span>
                    </div>
                    <div className="w-full bg-gray-700 rounded-full h-2.5">
                        <div
                            className="h-2.5 rounded-full bg-purple-500"
                            style={{ width: `${Math.min(100, Math.max(0, data.volatilityScore))}%` }}
                        ></div>
                    </div>
                </div>

                {/* Trend Score */}
                <div className="mb-4">
                    <div className="flex justify-between mb-1">
                        <span className="text-sm font-medium text-gray-300">Trend Risk</span>
                        <span className="text-sm font-medium text-white">{data.trendScore.toFixed(1)}</span>
                    </div>
                    <div className="w-full bg-gray-700 rounded-full h-2.5">
                        <div
                            className="h-2.5 rounded-full bg-blue-500"
                            style={{ width: `${Math.min(100, Math.max(0, data.trendScore))}%` }}
                        ></div>
                    </div>
                </div>

                {/* Volume Score */}
                <div className="mb-4">
                    <div className="flex justify-between mb-1">
                        <span className="text-sm font-medium text-gray-300">Volume Risk</span>
                        <span className="text-sm font-medium text-white">{data.volumeScore.toFixed(1)}</span>
                    </div>
                    <div className="w-full bg-gray-700 rounded-full h-2.5">
                        <div
                            className="h-2.5 rounded-full bg-orange-500"
                            style={{ width: `${Math.min(100, Math.max(0, data.volumeScore))}%` }}
                        ></div>
                    </div>
                </div>
            </div>
        </div>
    );
};
