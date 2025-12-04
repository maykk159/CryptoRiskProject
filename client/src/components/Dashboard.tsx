import React, { useEffect, useState } from 'react';
// import { AssetSelector } from './AssetSelector';
import { RiskCard } from './RiskCard';
// import { PriceChart } from './PriceChart';
// import { getRiskAnalysis } from '../services/api';
import { RiskAnalysisResponse } from '../types';

const ASSETS = [
    { id: 'bitcoin', name: 'Bitcoin (BTC)' },
    { id: 'ethereum', name: 'Ethereum (ETH)' },
    { id: 'solana', name: 'Solana (SOL)' },
    { id: 'ripple', name: 'XRP (XRP)' },
    { id: 'cardano', name: 'Cardano (ADA)' },
];

const AssetSelector: React.FC<{ selectedAsset: string; onSelectAsset: (asset: string) => void }> = ({ selectedAsset, onSelectAsset }) => {
    return (
        <div className="mb-6">
            <label htmlFor="asset-select" className="block text-sm font-medium text-gray-300 mb-2">
                Select Crypto Asset
            </label>
            <select
                id="asset-select"
                value={selectedAsset}
                onChange={(e) => onSelectAsset(e.target.value)}
                className="block w-full px-4 py-3 rounded-lg bg-gray-800 border border-gray-700 text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
            >
                {ASSETS.map((asset) => (
                    <option key={asset.id} value={asset.id}>
                        {asset.name}
                    </option>
                ))}
            </select>
        </div>
    );
};

export const Dashboard: React.FC = () => {
    const [selectedAsset, setSelectedAsset] = useState<string>('bitcoin');
    const [data, setData] = useState<RiskAnalysisResponse | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    // useEffect(() => {
    //     const fetchData = async () => {
    //         setLoading(true);
    //         setError(null);
    //         try {
    //             const result = await getRiskAnalysis(selectedAsset);
    //             setData(result);
    //         } catch (err) {
    //             console.error(err);
    //             setError('Failed to fetch risk analysis data. Please try again later.');
    //         } finally {
    //             setLoading(false);
    //         }
    //     };

    //     fetchData();
    // }, [selectedAsset]);

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

                {/* <AssetSelector selectedAsset={selectedAsset} onSelectAsset={setSelectedAsset} /> */}

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
                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                        {/* <RiskCard data={data} /> */}
                        {/* <PriceChart data={data.priceHistory} /> */}
                    </div>
                )}
            </div>
        </div>
    );
};
