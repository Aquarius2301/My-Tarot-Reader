import { lazy, Suspense } from "react";
import { Spin } from "antd";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import {
  PublicRoute,
  ProtectedRoute,
  RouteTitle,
  SessionExpiredHandler,
} from "./components";
import { MainLayout } from "@/components";
import { WEB_URL } from "./url.routes";

const HomePage = lazy(() => import("@/pages/auth/HomePage"));
const GuestHomePage = lazy(() => import("@/pages/guest/HomePage"));
const LoginPage = lazy(() => import("@/pages/guest/LoginPage"));
const LoginCallbackPage = lazy(() => import("@/pages/guest/LoginCallbackPage"));
const GuestDrawTarotPage = lazy(() => import("@/pages/guest/TarotPage"));
const DrawTarotPage = lazy(() => import("@/pages/auth/TarotPage"));
const LibraryPage = lazy(() => import("@/pages/auth/LibraryPage"));
const HistoryPage = lazy(() => import("@/pages/auth/HistoryPage/HistoryPage"));

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
      <Suspense
        fallback={
          <MainLayout>
            <Spin fullscreen />
          </MainLayout>
        }
      >
        <Routes>
          {/* Public routes render inside PublicRoute's MainLayout via <Outlet/>.
            ProtectedRoute redirects unauthenticated users to WEB_URL.HOME. */}
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
    </BrowserRouter>
  );
}
