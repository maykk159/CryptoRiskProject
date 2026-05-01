import React from 'react';
import type { RiskAnalysisResponse } from '../types';
import clsx from 'clsx';
import { Activity, TrendingUp, BarChart2 } from 'lucide-react';

interface RiskCardProps {
    data: RiskAnalysisResponse;
}

const colorClasses = {
    purple: { iconBg: 'bg-purple-500/20', iconText: 'text-purple-400', barFill: 'bg-purple-500', barThumb: 'bg-purple-400', text: 'text-purple-400' },
    blue: { iconBg: 'bg-blue-500/20', iconText: 'text-blue-400', barFill: 'bg-blue-500', barThumb: 'bg-blue-400', text: 'text-blue-400' },
    orange: { iconBg: 'bg-orange-500/20', iconText: 'text-orange-400', barFill: 'bg-orange-500', barThumb: 'bg-orange-400', text: 'text-orange-400' },
};

const ScoreBar: React.FC<{ label: string; description: string; score: number; colorKey: keyof typeof colorClasses; icon: React.ReactNode }> = ({ label, description, score, colorKey, icon }) => {
    const getLevelText = (s: number) => {
        if (s < 30) return 'Low';
        if (s < 70) return 'Medium';
        return 'High';
    };

    const levelText = getLevelText(score);
    const theme = colorClasses[colorKey];

    return (
        <div className="flex items-center gap-4 bg-gray-900/40 p-4 rounded-xl border border-gray-700/50">
            {/* Icon */}
            <div className={clsx("p-3 rounded-xl shrink-0", theme.iconBg, theme.iconText)}>
                {icon}
            </div>

            {/* Texts */}
            <div className="flex-1 min-w-[140px]">
                <p className="text-white font-medium text-sm">{label}</p>
                <p className="text-gray-400 text-xs mt-0.5">{description}</p>
            </div>

            {/* Progress Bar */}
            <div className="flex-[2] hidden md:block mx-4">
                <div className="w-full bg-gray-700/50 rounded-full h-2.5">
                    <div
                        className={clsx("h-2.5 rounded-full relative", theme.barFill)}
                        style={{ width: `${Math.min(100, Math.max(0, score))}%` }}
                    >
                        <div className={clsx("absolute right-0 top-1/2 -translate-y-1/2 w-4 h-4 rounded-full shadow-sm", theme.barThumb)}></div>
                    </div>
                </div>
            </div>

            {/* Score & Badge */}
            <div className="text-right shrink-0 min-w-[60px] flex flex-col items-end gap-1">
                <span className={clsx("text-xl font-bold leading-none", theme.text)}>{score.toFixed(1)}</span>
                <span className={clsx("text-[10px] font-semibold px-2 py-0.5 rounded-full", theme.iconBg, theme.text)}>
                    {levelText}
                </span>
            </div>
        </div>
    );
};

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

            {/* The user explicitly asked NOT to touch the existing medium risk part (composite score container) */}
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

            <div className="space-y-4">
                <ScoreBar 
                    label="Volatility Risk" 
                    description="Price fluctuation and volatility analysis"
                    score={data.volatilityScore} 
                    colorKey="purple" 
                    icon={<Activity size={20} />}
                />
                <ScoreBar 
                    label="Trend Risk" 
                    description="Market trend and momentum analysis"
                    score={data.trendScore} 
                    colorKey="blue" 
                    icon={<TrendingUp size={20} />}
                />
                <ScoreBar 
                    label="Volume Risk" 
                    description="Trading volume and liquidity analysis"
                    score={data.volumeScore} 
                    colorKey="orange" 
                    icon={<BarChart2 size={20} />}
                />
            </div>
        </div>
    );
};
