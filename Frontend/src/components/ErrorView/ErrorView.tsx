import { useEffect, useState } from "react";
import { Button, Typography, theme } from "antd";
import { ReloadOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import type { ErrorProps, ErrorType } from "./types";
import { ERROR_CONFIG } from "./constants";

const { Title, Paragraph } = Typography;

export function ErrorView({ type, onRetry }: ErrorProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  // ---------- auto-detect offline when no explicit type is given ----------
  const [isOffline, setIsOffline] = useState(!navigator.onLine);

  useEffect(() => {
    if (type) return; // explicit type takes precedence — skip listener

    const handleOnline = () => setIsOffline(false);
    const handleOffline = () => setIsOffline(true);

    window.addEventListener("online", handleOnline);
    window.addEventListener("offline", handleOffline);
    return () => {
      window.removeEventListener("online", handleOnline);
      window.removeEventListener("offline", handleOffline);
    };
  }, [type]);

  const resolvedType: ErrorType = type ?? (isOffline ? "offline" : "generic");
  const { titleKey, descKey, Icon } = ERROR_CONFIG[resolvedType];

  // ---------- render ----------
  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        minHeight: "100%",
        padding: "40px 16px",
        textAlign: "center",
      }}
    >
      <Icon
        style={{
          fontSize: 72,
          color: token.colorTextSecondary,
          marginBottom: 24,
        }}
      />

      <Title level={4} style={{ margin: 0, marginBottom: 8 }}>
        {t(titleKey)}
      </Title>

      <Paragraph
        type="secondary"
        style={{ maxWidth: 400, margin: 0, marginBottom: onRetry ? 24 : 0 }}
      >
        {t(descKey)}
      </Paragraph>

      {onRetry && (
        <Button
          type="primary"
          icon={<ReloadOutlined />}
          onClick={onRetry}
          size="large"
          style={{ marginTop: 24 }}
        >
          {t("component.error.retry")}
        </Button>
      )}
    </div>
  );
}
