import React from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import type { PriceData } from '../types';

interface PriceChartProps {
    data: PriceData[];
    timeRange: number;
}

export const PriceChart: React.FC<PriceChartProps> = ({ data, timeRange }) => {
    const formattedData = data.map(d => {
        const dateObj = new Date(d.timestamp);
        return {
            ...d,
            date: dateObj.toLocaleDateString(), // Keep full date for tooltip or reference
            day: dateObj.getDate(), // robust day extraction
            fullDate: dateObj // keep object if needed
        };
    });

    return (
        <div className="bg-gray-800 rounded-xl p-6 shadow-lg border border-gray-700 min-h-[500px]">
            <h2 className="text-xl font-bold text-white mb-6">{timeRange}-Day Price History</h2>
            <ResponsiveContainer width="100%" height="100%">
                <LineChart data={formattedData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                    <XAxis
                        dataKey="timestamp"
                        stroke="#9CA3AF"
                        tick={{ fill: '#9CA3AF' }}
                        tickFormatter={(timestamp) => {
                            const date = new Date(timestamp);
                            return `${date.getDate()}/${date.getMonth() + 1}`; // Display DD/M
                        }}
                        minTickGap={50}
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
                        labelFormatter={(label) => new Date(label).toLocaleDateString(undefined, {
                            weekday: 'short',
                            year: 'numeric',
                            month: 'short',
                            day: 'numeric'
                        })}
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
