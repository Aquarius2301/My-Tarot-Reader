import { App, Col, Typography, theme } from "antd";
import { GoogleLogin } from "@react-oauth/google";
import { useTranslation } from "react-i18next";

const { Title, Paragraph } = Typography;

interface GoogleLoginPanelProps {
  welcomeTitle: string;
  welcomeSubtitle: string;
  onLogin: (credential: string) => void;
}

export default function GoogleLoginPanel({
  welcomeTitle,
  welcomeSubtitle,
  onLogin,
}: GoogleLoginPanelProps) {
  const { message } = App.useApp();
  const { t } = useTranslation();
  const { token } = theme.useToken();

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

        <GoogleLogin
          onSuccess={(response) => {
            if (response.credential) {
              onLogin(response.credential);
            }
          }}
          onError={() => message.error(t("page.login.googleLoginError"))}
          theme="outline"
          size="large"
          shape="pill"
          text="continue_with"
          width="100%"
          useOneTap={false}
        />
      </div>
    </Col>
  );
}
