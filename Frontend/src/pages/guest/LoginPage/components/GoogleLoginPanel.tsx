import { Button, Col, Flex, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import { WEB_URL } from "@/routes";

const { Title, Paragraph } = Typography;

interface GoogleLoginPanelProps {
  welcomeTitle: string;
  welcomeSubtitle: string;
}

/**
 * Builds a Google OAuth authorization URL for the implicit (OpenID Connect)
 * flow. The id_token is returned in the URL fragment (GET) so the SPA can
 * pick it up without any server-side form POST — unlike GIS's `ux_mode:
 * "redirect"`, which POSTs to the login_uri and therefore 404s on a static Vite
 * SPA. A full-page `window.location.assign` also sidesteps popup blockers.
 */
function buildGoogleAuthUrl(clientId: string, redirectUri: string, nonce: string): string {
  const params = new URLSearchParams({
    client_id: clientId,
    redirect_uri: redirectUri,
    response_type: "id_token",
    scope: "openid email profile",
    nonce,
    // Skip the consent screen if the user has already granted permission.
    prompt: "select_account",
  });

  return `https://accounts.google.com/o/oauth2/v2/auth?${params.toString()}`;
}

function GoogleIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 48 48" aria-hidden="true">
      <path
        fill="#FFC107"
        d="M43.611 20.083H42V20H24v8h11.303c-1.649 4.657-6.08 8-11.303 8-6.627 0-12-5.373-12-12s5.373-12 12-12c3.059 0 5.842 1.154 7.961 3.039l5.657-5.657C34.046 6.053 29.268 4 24 4 12.955 4 4 12.955 4 24s8.955 20 20 20 20-8.955 20-20c0-1.341-.138-2.65-.389-3.917z"
      />
      <path
        fill="#FF3D00"
        d="M6.306 14.691l6.571 4.819C14.655 15.108 18.961 12 24 12c3.059 0 5.842 1.154 7.961 3.039l5.657-5.657C34.046 6.053 29.268 4 24 4 16.318 4 9.656 8.337 6.306 14.691z"
      />
      <path
        fill="#4CAF50"
        d="M24 44c5.163 0 9.879-1.979 13.399-5.198l-6.157-5.219A11.953 11.953 0 0 1 24 36c-5.185 0-9.571-3.418-11.162-8.078l-6.424 4.944C10.95 39.61 16.915 44 24 44z"
      />
      <path
        fill="#1976D2"
        d="M43.611 20.083H42V20H24v8h11.303a12.04 12.04 0 0 1-4.087 5.571l.003-.002 6.157 5.219C38.971 39.205 44 34 44 24c0-1.341-.138-2.65-.389-3.917z"
      />
    </svg>
  );
}

export default function GoogleLoginPanel({
  welcomeTitle,
  welcomeSubtitle,
}: GoogleLoginPanelProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const handleGoogleRedirect = () => {
    const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
    const redirectUri = `${window.location.origin}${WEB_URL.loginCallback}`;
    const nonce = crypto.randomUUID();

    // Keep the nonce around so the callback page can verify the id_token
    // wasn't replayed, then navigate away — a plain GET redirect that defeats
    // popup blockers.
    sessionStorage.setItem("google_oauth_nonce", nonce);

    window.location.assign(buildGoogleAuthUrl(clientId, redirectUri, nonce));
  };

  return (
    <Col
      xs={24}
      md={12}
      style={{
        padding: `${token.paddingXL}px ${token.paddingLG}px`,
        display: "flex",
        flexDirection: "column",
        justifyContent: "center",
        alignItems: "center",
        textAlign: "center",
      }}
    >
      <div style={{ width: "100%", maxWidth: 320 }}>
        <Title level={4} style={{ marginBottom: 8 }}>
          {welcomeTitle}
        </Title>
        <Paragraph type="secondary" style={{ marginBottom: 32 }}>
          {welcomeSubtitle}
        </Paragraph>

        <Flex justify="center" style={{ width: "100%" }}>
          <Button
            size="large"
            shape="round"
            style={{
              height: 44,
              width: "100%",
              maxWidth: 250,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              gap: 10,
              fontSize: 15,
            }}
            onClick={handleGoogleRedirect}
          >
            <GoogleIcon />
            {t("page.login.googleSignIn")}
          </Button>
        </Flex>
      </div>
    </Col>
  );
}