import { App, Card, Row, Spin, theme } from "antd";
import {
  CompassOutlined, // Replaces SparklesOutlined
  StarOutlined,
  HistoryOutlined,
} from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useLogin } from "@/hooks/api";
import { BenefitsPanel, GoogleLoginPanel } from "./components";
import { useNavigate } from "react-router-dom";
import { WEB_URL } from "@/constants";
import type { ReactNode } from "react";

// Interface to avoid the 'readonly' issue with Antd List
export interface BenefitItem {
  key: string;
  icon: ReactNode;
  title: string;
  description: string;
}

export default function LoginPage() {
  const { t } = useTranslation();
  const { mutate, isPending } = useLogin();
  const navigate = useNavigate();
  const { token } = theme.useToken();
  const { message } = App.useApp();

  const handleGoogleLogin = (credential: string) => {
    mutate(
      { credential },
      {
        onSuccess: () => {
          navigate(WEB_URL.AUTH_HOME, { replace: true });
        },
        onError: () => {
          message.error(t("page.login.googleLoginError"));
        },
      },
    );
  };

  const loginBenefits: BenefitItem[] = [
    {
      key: "ai",
      icon: (
        <StarOutlined style={{ color: token.colorPrimary, fontSize: 20 }} />
      ),
      title: t("page.login.benefit.ai.title"),
      description: t("page.login.benefit.ai.description"),
    },
    {
      key: "history",
      icon: (
        <HistoryOutlined
          style={{ color: token.colorTextSecondary, fontSize: 20 }}
        />
      ),
      title: t("page.login.benefit.history.title"),
      description: t("page.login.benefit.history.description"),
    },
    {
      key: "daily",
      icon: (
        <CompassOutlined style={{ color: token.colorWarning, fontSize: 20 }} />
      ),
      title: t("page.login.benefit.daily.title"),
      description: t("page.login.benefit.daily.description"),
    },
  ];

  return (
    <div style={{ maxWidth: 900, margin: "40px auto", padding: "0 16px" }}>
      <Card
        style={{
          borderRadius: token.borderRadiusLG,
          overflow: "hidden",
          boxShadow: token.boxShadowSecondary,
        }}
        styles={{ body: { padding: 0 } }}
      >
        <Row>
          <BenefitsPanel
            benefits={loginBenefits}
            heading={t("page.login.heading")}
            subtitle={t("page.login.subtitle")}
          />

          <GoogleLoginPanel
            welcomeTitle={t("page.login.welcomeTitle")}
            welcomeSubtitle={t("page.login.welcomeSubtitle")}
            onLogin={handleGoogleLogin}
          />
        </Row>
      </Card>

      {isPending && <Spin fullscreen />}
    </div>
  );
}
