import React from 'react';
import type { PriceData } from '../../types';

interface PriceHistoryInfoProps {
    timeRange: number;
    priceHistory: PriceData[];
}

export const PriceHistoryInfo: React.FC<PriceHistoryInfoProps> = ({ timeRange, priceHistory }) => {
    const latestPrice = priceHistory.length > 0
        ? priceHistory[priceHistory.length - 1]?.price
        : 0;

    return (
        <div className="bg-gray-800 rounded-xl p-6 shadow-lg border border-gray-700">
            <h2 className="text-xl font-bold text-white mb-6">{timeRange}-Day Price History</h2>
            <p className="text-gray-400">
                Data points: {priceHistory.length}
            </p>
            <p className="text-gray-400">
                Latest price: ${latestPrice.toLocaleString()}
            </p>
        </div>
    );
};
