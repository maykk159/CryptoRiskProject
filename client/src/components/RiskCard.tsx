import React from 'react';
import { RiskAnalysisResponse } from '../types';
import clsx from 'clsx';

interface RiskCardProps {
    data: RiskAnalysisResponse;
}

const ScoreBar: React.FC<{ label: string; score: number; colorClass: string }> = ({ label, score, colorClass }) => (
    <div className="mb-4">
        <div className="flex justify-between mb-1">
            <span className="text-sm font-medium text-gray-300">{label}</span>
            <span className="text-sm font-medium text-white">{score.toFixed(1)}</span>
        </div>
        <div className="w-full bg-gray-700 rounded-full h-2.5">
            <div
                className={clsx("h-2.5 rounded-full", colorClass)}
                style={{ width: `${Math.min(100, Math.max(0, score))}%` }}
            ></div>
        </div>
    </div>
);

export const RiskCard: React.FC<RiskCardProps> = ({ data }) => {
    const getRiskLevel = (score: number) => {
        if (score < 30) return { text: 'Low Risk', color: 'text-green-400' };
        if (score < 70) return { text: 'Medium Risk', color: 'text-yellow-400' };
        return { text: 'High Risk', color: 'text-red-400' };
    };

    const riskLevel = getRiskLevel(data.compositeRiskScore);

    return (
        <div className="bg-gray-800 rounded-xl p-6 shadow-lg border border-gray-700">
            <h2 className="text-xl font-bold text-white mb-6">Risk Analysis</h2>

            <div className="flex items-center justify-between mb-8 p-4 bg-gray-900 rounded-lg">
                <div>
                    <p className="text-gray-400 text-sm">Composite Risk Score</p>
                    <p className={clsx("text-3xl font-bold", riskLevel.color)}>
                        {data.compositeRiskScore.toFixed(1)}
                    </p>
                </div>
                <div className={clsx("text-lg font-semibold px-4 py-2 rounded-full bg-opacity-20", riskLevel.color.replace('text-', 'bg-'))}>
                    {riskLevel.text}
                </div>
            </div>

            <div className="space-y-6">
                <ScoreBar label="Volatility Risk" score={data.volatilityScore} colorClass="bg-purple-500" />
                <ScoreBar label="Trend Risk" score={data.trendScore} colorClass="bg-blue-500" />
                <ScoreBar label="Volume Risk" score={data.volumeScore} colorClass="bg-orange-500" />
            </div>
        </div>
    );
};
