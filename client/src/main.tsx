import React from 'react';
import ReactDOM from 'react-dom/client';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'react-hot-toast';
import App from './App.tsx';
import './index.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,              // 1 retry on failure (Polly handles retries on the server side)
      staleTime: 60_000,     // 1 minute — matches backend cache duration
      refetchOnWindowFocus: false, // don't re-fetch when switching tabs
    },
  },
});

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
      <Toaster
        position="bottom-right"
        toastOptions={{
          style: {
            background: '#1f2937',
            color: '#f3f4f6',
            border: '1px solid #374151',
          },
          success: { iconTheme: { primary: '#22c55e', secondary: '#1f2937' } },
          error: { iconTheme: { primary: '#ef4444', secondary: '#1f2937' } },
        }}
      />
    </QueryClientProvider>
  </React.StrictMode>
);
