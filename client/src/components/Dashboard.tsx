import React, { useEffect, useState } from 'react';
import { getRiskAnalysis } from '../services/api';
import type { RiskAnalysisResponse } from '../types';

const ASSETS = [
    { id: 'bitcoin', name: 'Bitcoin (BTC)' },
    { id: 'ethereum', name: 'Ethereum (ETH)' },
    { id: 'solana', name: 'Solana (SOL)' },
    { id: 'ripple', name: 'Ripple (XRP)' },
    { id: 'cardano', name: 'Cardano (ADA)' },
    { id: 'binancecoin', name: 'BNB (BNB)' },
    { id: 'dogecoin', name: 'Dogecoin (DOGE)' },
];

export const Dashboard: React.FC = () => {
    const [selectedAsset, setSelectedAsset] = useState<string>('bitcoin');
    const [data, setData] = useState<RiskAnalysisResponse | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
            setError(null);
            try {
                const result = await getRiskAnalysis(selectedAsset);
                setData(result);
            } catch (err: any) {
                console.error('API Error:', err);
                const errorMsg = err.response?.status === 429
                    ? 'API rate limit exceeded. Please wait a few seconds and try again.'
                    : err.response?.status === 404
                        ? `Crypto asset "${selectedAsset}" not found. Please select a different asset.`
                        : 'Failed to fetch risk analysis data. Please check your internet connection and try again.';
                setError(errorMsg);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [selectedAsset]);

    const getRiskLevel = (score: number) => {
        if (score < 30) return { text: 'Low Risk', color: 'text-green-400', bgColor: 'bg-green-400' };
        if (score < 70) return { text: 'Medium Risk', color: 'text-yellow-400', bgColor: 'bg-yellow-400' };
        return { text: 'High Risk', color: 'text-red-400', bgColor: 'bg-red-400' };
    };

    return (
        <div className="min-h-screen bg-gray-900 text-white p-8">
            <div className="max-w-7xl mx-auto">
                <header className="mb-10">
                    <h1 className="text-4xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-blue-400 to-purple-500">
                        Crypto Risk Analysis
                    </h1>
                    <p className="text-gray-400 mt-2">
                        Advanced financial risk assessment for crypto assets
                    </p>
                </header>

                {/* Asset Selector */}
                <div className="mb-6">
                    <label htmlFor="asset-select" className="block text-sm font-medium text-gray-300 mb-2">
                        Select Crypto Asset
                    </label>
                    <select
                        id="asset-select"
                        value={selectedAsset}
                        onChange={(e) => setSelectedAsset(e.target.value)}
                        className="block w-full px-4 py-3 rounded-lg bg-gray-800 border border-gray-700 text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                    >
                        {ASSETS.map((asset) => (
                            <option key={asset.id} value={asset.id}>
                                {asset.name}
                            </option>
                        ))}
                    </select>
                </div>

                {loading && (
                    <div className="flex justify-center items-center h-64">
                        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
                    </div>
                )}

                {error && (
                    <div className="bg-red-900/50 border border-red-500 text-red-200 p-4 rounded-lg mb-6">
                        {error}
                    </div>
                )}

                {!loading && !error && data && (
                    <div className="grid grid-cols-1 gap-8">
                        {/* Risk Card */}
                        <div className="bg-gray-800 rounded-xl p-6 shadow-lg border border-gray-700">
                            <h2 className="text-xl font-bold text-white mb-6">Risk Analysis</h2>

                            <div className="flex items-center justify-between mb-8 p-4 bg-gray-900 rounded-lg">
                                <div>
                                    <p className="text-gray-400 text-sm">Composite Risk Score</p>
                                    <p className={`text-3xl font-bold ${getRiskLevel(data.compositeRiskScore).color}`}>
                                        {data.compositeRiskScore.toFixed(1)}
                                    </p>
                                </div>
                                <div className={`text-lg font-semibold px-4 py-2 rounded-full ${getRiskLevel(data.compositeRiskScore).color}`}>
                                    {getRiskLevel(data.compositeRiskScore).text}
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

                        {/* Price History Info */}
                        <div className="bg-gray-800 rounded-xl p-6 shadow-lg border border-gray-700">
                            <h2 className="text-xl font-bold text-white mb-6">30-Day Price History</h2>
                            <p className="text-gray-400">
                                Data points: {data.priceHistory.length}
                            </p>
                            <p className="text-gray-400">
                                Latest price: ${data.priceHistory[data.priceHistory.length - 1]?.price.toLocaleString()}
                            </p>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};
