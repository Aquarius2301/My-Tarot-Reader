import { App, Spin } from "antd";
import { useEffect, useRef } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { useLogin } from "@/hooks/api";
import { WEB_URL } from "@/routes";

function decodeJwtPayload(token: string): { nonce?: string } | null {
  try {
    const base64Url = token.split(".")[1];
    if (!base64Url) return null;

    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const padded = base64.padEnd(
      base64.length + ((4 - (base64.length % 4)) % 4),
      "=",
    );

    return JSON.parse(atob(padded));
  } catch {
    return null;
  }
}

/**
 * Landing page for Google's redirect (ux_mode="redirect") OAuth flow.
 *
 * With a full-page redirect Google delivers the sign-in result back as a URL
 * fragment here (`#id_token=...` or `#error=...`) instead of invoking a popup
 * callback — this is what keeps sign-in working under ad/popup blockers.
 * The credential is forwarded to the backend, which sets the HttpOnly session
 * cookies, and then we move on to the home page.
 */
export default function LoginCallbackPage() {
  const { message } = App.useApp();
  const { i18n } = useTranslation();
  const navigate = useNavigate();
  const { mutate } = useLogin();
  const handled = useRef(false);

  useEffect(() => {
    // Guard against StrictMode double-invoking this effect in development.
    if (handled.current) return;
    handled.current = true;

    const fragment = new URLSearchParams(window.location.hash.slice(1));

    // Never leave the id_token sitting in the address bar.
    window.history.replaceState({}, "", window.location.pathname);

    const error = fragment.get("error");
    if (error) {
      message.error(i18n.t("page.login.googleLoginError"));
      navigate(WEB_URL.login, { replace: true });
      return;
    }

    const idToken = fragment.get("id_token");
    if (!idToken) {
      navigate(WEB_URL.login, { replace: true });
      return;
    }

    // Best-effort replay protection: the nonce we generated before redirecting
    // must match the one embedded in the id_token. The backend independently
    // validates the JWT, so a mismatch here just bounces back to the login page.
    const expectedNonce = sessionStorage.getItem("google_oauth_nonce");
    sessionStorage.removeItem("google_oauth_nonce");

    const payload = decodeJwtPayload(idToken);
    if (!payload) {
      navigate(WEB_URL.login, { replace: true });
      return;
    }
    const tokenNonce = payload.nonce;

    if (!expectedNonce || expectedNonce !== tokenNonce) {
      navigate(WEB_URL.login, { replace: true });
      return;
    }

    mutate(
      { credential: idToken, locale: i18n.language },
      {
        onSuccess: () => navigate(WEB_URL.home, { replace: true }),
        onError: () => {
          message.error(i18n.t("page.login.googleLoginError"));
          navigate(WEB_URL.login, { replace: true });
        },
      },
    );

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [i18n, message, mutate, navigate]);

  return <Spin fullscreen />;
}
