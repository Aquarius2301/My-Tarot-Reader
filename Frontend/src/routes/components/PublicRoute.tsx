import { Spin } from "antd";
import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useGetCurrentUser } from "@/hooks/api";
import { WEB_URL } from "../url.routes";

// When the user is already authenticated, guest-only pages redirect to the
// matching auth page. /login/callback is intentionally NOT mapped: the OAuth
// flow re-authenticates from the id_token even if a stale session exists.
const GUEST_TO_AUTH_REDIRECT: Record<string, string> = {
  [WEB_URL.guestHome]: WEB_URL.home,
  [WEB_URL.guestTarot]: WEB_URL.tarot,
};

export default function PublicLayout() {
  const location = useLocation();

  // Only probe the session on pages that map an authenticated user to the auth
  // equivalent. /login (and its callback) need nothing and would otherwise 401
  // -> trigger an unnecessary /auth/refresh for guests.
  const isAuthFreeRoute =
    location.pathname === WEB_URL.login ||
    location.pathname === WEB_URL.loginCallback;

  const { data, isLoading } = useGetCurrentUser(!isAuthFreeRoute);

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (data) {
    const redirectTo = GUEST_TO_AUTH_REDIRECT[location.pathname];
    if (redirectTo) return <Navigate to={redirectTo} replace />;
  }

  return <Outlet />;
}
