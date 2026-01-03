import React, { useEffect, useState } from 'react';
import { getRiskAnalysis } from '../services/api';
import type { RiskAnalysisResponse } from '../types';
import { ASSETS } from '../constants/assets';
import { TimeRangeSelector } from './dashboard/TimeRangeSelector';
import { RiskScoreCard } from './dashboard/RiskScoreCard';
import { AdvancedMetrics } from './dashboard/AdvancedMetrics';
import { PriceChart } from './PriceChart';

export const Dashboard: React.FC = () => {
    const [selectedAssetId, setSelectedAssetId] = useState<string>('bitcoin');
    const selectedAsset = ASSETS.find(a => a.id === selectedAssetId) || ASSETS[0];
    const [selectedTimeRange, setSelectedTimeRange] = useState<number>(30);
    const [data, setData] = useState<RiskAnalysisResponse | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
            setError(null);
            try {
                const result = await getRiskAnalysis(selectedAssetId, selectedTimeRange);
                setData(result);
            } catch (err: any) {
                console.error('API Error:', err);

                // Prioritize the error message coming from our API wrapper
                if (err.message && err.message !== 'Network Error') {
                    setError(err.message);
                }
                // Handle Axios specific response errors
                else if (err.response) {
                    const status = err.response.status;
                    if (status === 429) {
                        setError('API rate limit exceeded. Please wait a few seconds and try again.');
                    } else if (status === 404) {
                        setError(`Crypto asset "${selectedAsset.name}" not found. Please select a different asset.`);
                    } else {
                        setError('Failed to fetch data from the server.');
                    }
                } else {
                    setError('Failed to connect to the server. Please check your internet connection.');
                }
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [selectedAssetId, selectedTimeRange]);

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
                        value={selectedAssetId}
                        onChange={(e) => setSelectedAssetId(e.target.value)}
                        className="block w-full px-4 py-3 rounded-lg bg-gray-800 border border-gray-700 text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                    >
                        {ASSETS.map((asset) => (
                            <option key={asset.id} value={asset.id}>
                                {asset.name} ({asset.ticker})
                            </option>
                        ))}
                    </select>
                </div>

                {/* Time Range Selector */}
                <TimeRangeSelector value={selectedTimeRange} onChange={setSelectedTimeRange} />

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
                        <RiskScoreCard data={data} asset={selectedAsset} />
                        <AdvancedMetrics data={data} />
                        <PriceChart data={data.priceHistory} timeRange={selectedTimeRange} />
                    </div>
                )}
            </div>
        </div>
    );
};
