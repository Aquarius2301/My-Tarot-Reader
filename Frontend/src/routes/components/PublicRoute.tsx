import { Spin } from "antd";
import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useGetCurrentUser } from "@/hooks/api";
import { WEB_URL } from "../url.routes";

// When the user is already authenticated, guest-only pages redirect to the
// matching auth page. /login/callback is intentionally NOT mapped: the OAuth
// flow re-authenticates from the id_token even if a stale session exists.
const GUEST_TO_AUTH_REDIRECT: Record<string, string> = {
  [WEB_URL.guestHome]: WEB_URL.home,
  [WEB_URL.login]: WEB_URL.home,
  [WEB_URL.guestTarot]: WEB_URL.tarot,
};

export default function PublicLayout() {
  const { data, isLoading } = useGetCurrentUser();
  const location = useLocation();

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (data) {
    const redirectTo = GUEST_TO_AUTH_REDIRECT[location.pathname];
    if (redirectTo) return <Navigate to={redirectTo} replace />;
  }

  return <Outlet />;
}