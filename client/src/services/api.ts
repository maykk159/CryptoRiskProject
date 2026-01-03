import axios from 'axios';
import type { ApiResponse, RiskAnalysisResponse } from '../types/index';

const API_URL = 'http://localhost:5058/api';

export const api = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

export const getRiskAnalysis = async (
    assetId: string,
    days: number = 30
): Promise<RiskAnalysisResponse> => {
    const response = await api.get<ApiResponse<RiskAnalysisResponse>>(`/RiskAnalysis/${assetId}?days=${days}`);

    if (!response.data.succeeded || !response.data.data) {
        throw new Error(response.data.message || 'Failed to fetch risk analysis data');
    }

    return response.data.data;
};
