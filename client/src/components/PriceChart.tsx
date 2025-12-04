import React from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { PriceData } from '../types';

interface PriceChartProps {
    data: PriceData[];
}

export const PriceChart: React.FC<PriceChartProps> = ({ data }) => {
    const formattedData = data.map(d => ({
        ...d,
        date: new Date(d.timestamp).toLocaleDateString(),
        price: d.price
    }));

    return (
        <div className="bg-gray-800 rounded-xl p-6 shadow-lg border border-gray-700 h-[400px]">
            <h2 className="text-xl font-bold text-white mb-6">30-Day Price History</h2>
            <ResponsiveContainer width="100%" height="100%">
                <LineChart data={formattedData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                    <XAxis
                        dataKey="date"
                        stroke="#9CA3AF"
                        tick={{ fill: '#9CA3AF' }}
                        tickFormatter={(value) => value.split('/')[1]} // Show day only to save space
                    />
                    <YAxis
                        stroke="#9CA3AF"
                        tick={{ fill: '#9CA3AF' }}
                        domain={['auto', 'auto']}
                        tickFormatter={(value) => `$${value.toLocaleString()}`}
                    />
                    <Tooltip
                        contentStyle={{ backgroundColor: '#1F2937', borderColor: '#374151', color: '#F3F4F6' }}
                        itemStyle={{ color: '#60A5FA' }}
                        formatter={(value: number) => [`$${value.toLocaleString()}`, 'Price']}
                    />
                    <Line
                        type="monotone"
                        dataKey="price"
                        stroke="#3B82F6"
                        strokeWidth={2}
                        dot={false}
                        activeDot={{ r: 8 }}
                    />
                </LineChart>
            </ResponsiveContainer>
        </div>
    );
};
