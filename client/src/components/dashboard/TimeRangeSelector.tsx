import React from 'react';

interface TimeRangeSelectorProps {
    value: number;
    onChange: (days: number) => void;
}

export const TimeRangeSelector: React.FC<TimeRangeSelectorProps> = ({ value, onChange }) => {
    const timeRanges = [
        { days: 7, label: '7 Days' },
        { days: 30, label: '30 Days' },
        { days: 90, label: '90 Days' }
    ];

    return (
        <div className="mb-6">
            <label className="block text-sm font-medium text-gray-300 mb-2">
                Analysis Period
            </label>
            <div className="flex gap-2">
                {timeRanges.map(({ days, label }) => (
                    <button
                        key={days}
                        onClick={() => onChange(days)}
                        className={`px-4 py-2 rounded-lg font-medium transition-colors ${value === days
                                ? 'bg-blue-500 text-white'
                                : 'bg-gray-800 text-gray-300 hover:bg-gray-700'
                            }`}
                    >
                        {label}
                    </button>
                ))}
            </div>
        </div>
    );
};
