import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { getRiskAnalysis, getErrorMessage } from '../services/api';
import { ASSETS } from '../constants/assets';
import { TimeRangeSelector } from './dashboard/TimeRangeSelector';
import { AssetSelector } from './AssetSelector';
import { RiskScoreCard } from './dashboard/RiskScoreCard';
import { AdvancedMetrics } from './dashboard/AdvancedMetrics';
import { PriceChart } from './PriceChart';

export const Dashboard: React.FC = () => {
  const [selectedAssetId, setSelectedAssetId] = useState<string>('bitcoin');
  const [selectedTimeRange, setSelectedTimeRange] = useState<number>(30);

  const selectedAsset = ASSETS.find(a => a.id === selectedAssetId) ?? ASSETS[0];

  // Replaces: useEffect + useState(loading) + useState(error) + useState(data) + useCallback + axios
  // Benefits: automatic cache, deduped requests, background refetch, proper loading/error states
  const { data, isLoading, error } = useQuery({
    queryKey: ['risk', selectedAssetId, selectedTimeRange],
    queryFn: () => getRiskAnalysis(selectedAssetId, selectedTimeRange),
  });

  // Type-safe error message — no more catch (err: any)
  const errorMessage = error ? getErrorMessage(error, selectedAsset.name) : null;

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
        <AssetSelector
          selectedAsset={selectedAssetId}
          onSelectAsset={setSelectedAssetId}
        />

        {/* Time Range Selector */}
        <TimeRangeSelector value={selectedTimeRange} onChange={setSelectedTimeRange} />

        {/* Loading */}
        {isLoading && (
          <div className="flex justify-center items-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500" />
          </div>
        )}

        {/* Error */}
        {errorMessage && (
          <div className="bg-red-900/50 border border-red-500 text-red-200 p-4 rounded-lg mb-6">
            {errorMessage}
          </div>
        )}

        {/* Content */}
        {!isLoading && !errorMessage && data && (
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
