import React from 'react';

interface AssetSelectorProps {
    selectedAsset: string;
    onSelectAsset: (asset: string) => void;
}

const ASSETS = [
    { id: 'bitcoin', name: 'Bitcoin (BTC)' },
    { id: 'ethereum', name: 'Ethereum (ETH)' },
    { id: 'solana', name: 'Solana (SOL)' },
    { id: 'ripple', name: 'Ripple (XRP)' },
    { id: 'cardano', name: 'Cardano (ADA)' },
    { id: 'binancecoin', name: 'BNB (BNB)' },
    { id: 'dogecoin', name: 'Dogecoin (DOGE)' },
];

export const AssetSelector: React.FC<AssetSelectorProps> = ({ selectedAsset, onSelectAsset }) => {
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
