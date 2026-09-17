import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { AUTH_SESSION_EXPIRED_EVENT } from "@/constants";
import { AUTH_QUERY_KEY } from "@/hooks/api";
import { WEB_URL } from "../url.routes";

/**
 * Listens for the session-expired event (dispatched by the axios response
 * interceptor when a refresh-token retry fails) and redirects to the guest
 * home, dropping only the auth-scoped query cache.
 */
export default function SessionExpiredHandler() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  useEffect(() => {
    const handleSessionExpired = () => {
      queryClient.removeQueries({ queryKey: AUTH_QUERY_KEY });
      navigate(WEB_URL.guestHome, { replace: true });
    };

    window.addEventListener(AUTH_SESSION_EXPIRED_EVENT, handleSessionExpired);
    return () =>
      window.removeEventListener(
        AUTH_SESSION_EXPIRED_EVENT,
        handleSessionExpired,
      );
  }, [navigate, queryClient]);

  return null;
}
