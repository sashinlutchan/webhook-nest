import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import type { ReactNode } from 'react';


const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
          staleTime: 1000 * 60 * 5,
    gcTime: 1000 * 60 * 10,
      retry: (failureCount: number, error: unknown) => {
        if (error && typeof error === 'object' && 'status' in error) {
          const status = (error as any).status;
          if (
            status >= 400 &&
            status < 500 &&
            status !== 401 &&
            status !== 403
          ) {
            return false;
          }
        }
        return failureCount < 3;
      },
      refetchOnWindowFocus: false,
      refetchOnReconnect: true,
    },
    mutations: {
      retry: 1,
    },
  },
});

interface QueryProviderProps {
  children: ReactNode;
}

export function QueryProvider({ children }: QueryProviderProps) {
  return (
    <QueryClientProvider client={queryClient}>
      {children}

      {import.meta.env.DEV && <ReactQueryDevtools initialIsOpen={false} />}
    </QueryClientProvider>
  );
}
