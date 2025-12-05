import axios from 'axios';
import type { RiskAnalysisResponse } from '../types/index';

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
    const response = await api.get<RiskAnalysisResponse>(`/RiskAnalysis/${assetId}?days=${days}`);
    return response.data;
};
