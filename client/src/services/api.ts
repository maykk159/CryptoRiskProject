import axios, { AxiosError } from 'axios';
import type { ApiResponse, RiskAnalysisResponse } from '../types/index';

// Reads from .env.local in development — prevents hardcoded localhost in production
const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5058/api';

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
  const response = await api.get<ApiResponse<RiskAnalysisResponse>>(
    `/RiskAnalysis/${assetId}?days=${days}`
  );

  if (!response.data.succeeded || !response.data.data) {
    throw new Error(response.data.message ?? 'Failed to fetch risk analysis data');
  }

  return response.data.data;
};

/**
 * Extracts a user-friendly error message from any error type.
 * Replaces the `catch (err: any)` anti-pattern with proper type narrowing
 * so TypeScript can actually check our error handling logic.
 */
export function getErrorMessage(err: unknown, assetName?: string): string {
  if (err instanceof AxiosError) {
    const status = err.response?.status;

    if (status === 429)
      return 'API rate limit exceeded. Please wait a few seconds and try again.';

    if (status === 404)
      return `Crypto asset "${assetName ?? 'unknown'}" not found. Please select a different asset.`;

    if (err.message && err.message !== 'Network Error') return err.message;

    return 'Failed to fetch data from the server.';
  }

  if (err instanceof Error && err.message && err.message !== 'Network Error') {
    return err.message;
  }

  return 'Failed to connect to the server. Please check your internet connection.';
}
