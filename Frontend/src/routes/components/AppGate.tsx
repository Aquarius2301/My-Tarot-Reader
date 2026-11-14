import { Route, Routes, useLocation } from "react-router-dom";
import { publicRoutes, registerRoutes, WEB_URL } from "./url.routes";
import { useGetCurrentUser } from "@/hooks/api";
import { AppLoading } from "@/components";
import { NotFoundPage } from "@/pages/common";
import GuestRoute from "./GuestRoute";
import RegisterRoute from "./RegisterRoute";
import NotFoundLayoutRoute from "./NotFoundLayoutRoute";

export default function AppGate() {
  const location = useLocation();

  // /login and /login/callback don't depend on the session, so skip the auth
  // probe there: for a guest /me would 401 and trigger a pointless
  // /auth/refresh round-trip (the OAuth callback boots its own login flow).
  const isAuthFreeRoute =
    location.pathname === WEB_URL.login ||
    location.pathname === WEB_URL.loginCallback;

  const { isLoading, data, dataUpdatedAt, errorUpdatedAt } =
    useGetCurrentUser(!isAuthFreeRoute);

  // Block the first paint with a bare spinner until the auth state is known,
  // so the header never renders guest chrome and then flips to auth chrome.
  // Only applies on cold boot: once the query has settled, later refetches
  // (login/session-expired) keep the current UI instead of blanking it.
  const hasResolved = dataUpdatedAt > 0 || errorUpdatedAt > 0;

  if (isLoading && !hasResolved) {
    return <AppLoading />;
  }

  return (
    <Routes>
      {/* Public routes render inside PublicRoute's Outlet; ProtectedRoute
          redirects unauthenticated users to WEB_URL.guestHome. The shared
          MainLayout stays mounted so the header never flashes on navigation. */}
      <Route element={<GuestRoute />}>
        {publicRoutes.map((r) => {
          const Component = r.component;
          return <Route key={r.path} path={r.path} element={<Component />} />;
        })}
      </Route>
      <Route element={<RegisterRoute />}>
        {registerRoutes.map((r) => {
          const Component = r.component;
          return <Route key={r.path} path={r.path} element={<Component />} />;
        })}
      </Route>
      <Route element={<NotFoundLayoutRoute user={data} />}>
        <Route key="*" path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
