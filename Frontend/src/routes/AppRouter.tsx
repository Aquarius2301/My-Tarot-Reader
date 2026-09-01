import { lazy, Suspense } from "react";
import { Spin } from "antd";
import { WEB_URL } from "@/constants";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import { PublicRoute, ProtectedRoute, RouteTitle } from "./components";
import { MainLayout } from "@/layouts";

const LazyHomePage = lazy(() => import("@/pages/public/HomePage"));
const LazyAuthHomePage = lazy(() => import("@/pages/protected/AuthHomePage"));
const LazyLoginPage = lazy(() => import("@/pages/public/LoginPage"));
const LazyAuthDrawTarotPage = lazy(
  () => import("@/pages/protected/AuthDrawTarotPage"),
);
const LazyGuestDrawTarotPage = lazy(
  () => import("@/pages/public/GuestDrawTarotPage"),
);
const LazyHistoryPage = lazy(() => import("@/pages/protected/HistoryPage"));

interface AppRoute {
  titleKey: string;
  path: string;
  component: React.ComponentType;
}

const publicRoutes: AppRoute[] = [
  {
    titleKey: "page.home.title",
    path: WEB_URL.HOME,
    component: LazyHomePage,
  },
  {
    titleKey: "page.login.title",
    path: WEB_URL.LOGIN,
    component: LazyLoginPage,
  },
  {
    titleKey: "page.draw.title",
    path: WEB_URL.DRAW_GUEST,
    component: LazyGuestDrawTarotPage,
  },
];
const protectedRoutes: AppRoute[] = [
  {
    titleKey: "page.home.title",
    path: WEB_URL.AUTH_HOME,
    component: LazyAuthHomePage,
  },
  {
    titleKey: "page.draw.title",
    path: WEB_URL.DRAW,
    component: LazyAuthDrawTarotPage,
  },
  {
    titleKey: "page.history.title",
    path: WEB_URL.HISTORY,
    component: LazyHistoryPage,
  },
];

export default function AppRouter() {
  return (
    <BrowserRouter>
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
