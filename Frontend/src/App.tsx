import { App as AntdApp } from "antd";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AppRouter } from "./routes";
import { lazy } from "react";
import { GoogleOAuthProvider } from "@react-oauth/google";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // After 5 minutes, the data is considered stale
      refetchOnWindowFocus: false, // No refetch for switching tabs
    },
  },
});

const ReactQueryDevtools = lazy(() =>
  import("@tanstack/react-query-devtools").then((m) => ({
    default: m.ReactQueryDevtools,
  })),
);

export default function App() {
  return (
    <GoogleOAuthProvider clientId={import.meta.env.VITE_GOOGLE_CLIENT_ID}>
      <QueryClientProvider client={queryClient}>
        <AntdApp>
          <AppRouter />
        </AntdApp>
        <ReactQueryDevtools initialIsOpen={false} />
      </QueryClientProvider>
    </GoogleOAuthProvider>
  );
}
