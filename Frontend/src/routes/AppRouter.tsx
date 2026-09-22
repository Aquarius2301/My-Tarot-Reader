import { lazy, Suspense } from "react";
import { Spin } from "antd";
import {
  BrowserRouter,
  Route,
  Routes,
  useLocation,
} from "react-router-dom";
import {
  PublicRoute,
  ProtectedRoute,
  RouteTitle,
  SessionExpiredHandler,
} from "./components";
import { BootScreen, MainLayout } from "@/components";
import { useGetCurrentUser } from "@/hooks/api";
import { WEB_URL } from "./url.routes";

const HomePage = lazy(() => import("@/pages/auth/HomePage"));
const GuestHomePage = lazy(() => import("@/pages/guest/HomePage"));
const LoginPage = lazy(() => import("@/pages/guest/LoginPage"));
const LoginCallbackPage = lazy(() => import("@/pages/guest/LoginCallbackPage"));
const GuestDrawTarotPage = lazy(() => import("@/pages/guest/TarotPage"));
const DrawTarotPage = lazy(() => import("@/pages/auth/TarotPage"));
const AiTarotPage = lazy(() => import("@/pages/auth/AiTarotPage"));
const AiTarotResultPage = lazy(() => import("@/pages/auth/AiTarotResultPage"));
const HistoryAiTarotPage = lazy(() => import("@/pages/auth/HistoryAiTarotPage"));
const LibraryPage = lazy(() => import("@/pages/auth/LibraryPage"));
const HistoryPage = lazy(() => import("@/pages/auth/HistoryPage"));

interface AppRoute {
  titleKey: string;
  path: string;
  component: React.ComponentType;
}

const publicRoutes: AppRoute[] = [
  {
    titleKey: "page.home.title",
    path: WEB_URL.guestHome,
    component: GuestHomePage,
  },
  {
    titleKey: "page.login.title",
    path: WEB_URL.login,
    component: LoginPage,
  },
  {
    titleKey: "page.tarot.title",
    path: WEB_URL.guestTarot,
    component: GuestDrawTarotPage,
  },
  {
    titleKey: "page.login.title",
    path: WEB_URL.loginCallback,
    component: LoginCallbackPage,
  },
];
const protectedRoutes: AppRoute[] = [
  {
    titleKey: "page.home.title",
    path: WEB_URL.home,
    component: HomePage,
  },
  {
    titleKey: "page.tarot.title",
    path: WEB_URL.tarot,
    component: DrawTarotPage,
  },
  {
    titleKey: "page.aiTarot.title",
    path: WEB_URL.aiTarot,
    component: AiTarotPage,
  },
  {
    titleKey: "page.aiTarot.result.title",
    path: `${WEB_URL.aiTarotResult}/:readingId`,
    component: AiTarotResultPage,
  },
  {
    titleKey: "page.historyAiTarot.title",
    path: WEB_URL.aiTarotHistory,
    component: HistoryAiTarotPage,
  },
  {
    titleKey: "page.library.title",
    path: WEB_URL.library,
    component: LibraryPage,
  },
  {
    titleKey: "page.history.title",
    path: WEB_URL.history,
    component: HistoryPage,
  },
];

export default function AppRouter() {
  return (
    <BrowserRouter>
      <SessionExpiredHandler />
      <AppGate />
    </BrowserRouter>
  );
}

function AppGate() {
  const location = useLocation();

  // /login and /login/callback don't depend on the session, so skip the auth
  // probe there: for a guest /me would 401 and trigger a pointless
  // /auth/refresh round-trip (the OAuth callback boots its own login flow).
  const isAuthFreeRoute =
    location.pathname === WEB_URL.login ||
    location.pathname === WEB_URL.loginCallback;

  const { data, isLoading, dataUpdatedAt, errorUpdatedAt } =
    useGetCurrentUser(!isAuthFreeRoute);

  // Block the first paint with a bare spinner until the auth state is known,
  // so the header never renders guest chrome and then flips to auth chrome.
  // Only applies on cold boot: once the query has settled, later refetches
  // (login/session-expired) keep the current UI instead of blanking it.
  const hasResolved = dataUpdatedAt > 0 || errorUpdatedAt > 0;

  if (isLoading && !hasResolved) {
    return <BootScreen />;
  }

  return (
    <MainLayout user={data} role={data?.role} currentPath={location.pathname}>
      <Suspense fallback={<Spin fullscreen />}>
        <Routes>
          {/* Public routes render inside PublicRoute's Outlet; ProtectedRoute
            redirects unauthenticated users to WEB_URL.guestHome. The shared
            MainLayout stays mounted so the header never flashes on navigation. */}
          <Route element={<PublicRoute />}>
            {publicRoutes.map((r) => {
              const Component = r.component;

              return (
                <Route
                  key={r.path}
                  path={r.path}
                  element={
                    <RouteTitle titleKey={r.titleKey}>
                      <Component />
                    </RouteTitle>
                  }
                />
              );
            })}
          </Route>

          <Route element={<ProtectedRoute />}>
            {protectedRoutes.map((r) => {
              const Component = r.component;

              return (
                <Route key={r.path} path={r.path} element={<Component />} />
              );
            })}
          </Route>
        </Routes>
      </Suspense>
    </MainLayout>
  );
}