import { Col, Typography, Button, Space, theme } from "antd";
import { CrownOutlined, ReadOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import type { UserRole } from "@/types";
import { WEB_URL } from "@/routes";

const { Title, Paragraph } = Typography;

interface HeroContentProps {
  role?: UserRole;
}

export default function HeroContent({ role }: HeroContentProps) {
  const { token } = theme.useToken();
  const { t } = useTranslation();
  const navigate = useNavigate();

  return (
    <Col xs={24} md={14}>
      <Title
        level={1}
        style={{
          fontSize: "clamp(2rem, 4vw, 3rem)",
          marginBottom: 16,
          color: token.colorText,
          fontFamily: token.fontFamily,
        }}
      >
        {t("page.home.heroTitle")} <br />
        <span
          style={{
            color: token.colorPrimary,
            textShadow: `0 0 20px ${token.colorPrimary}66`,
          }}
        >
          {t("page.home.heroTitleHighlight")}
        </span>
      </Title>
      <Paragraph
        style={{
          fontSize: 16,
          color: token.colorTextSecondary,
          marginBottom: 28,
          maxWidth: 520,
        }}
      >
        {t("page.home.heroDescription")}
      </Paragraph>
      <Space size="middle" wrap>
        <Button
          onClick={() => {
            navigate(role ? WEB_URL.tarot : WEB_URL.guestTarot);
          }}
          type="primary"
          size="large"
          icon={<ReadOutlined />}
          style={{
            height: 48,
            padding: "0 28px",
            fontSize: 16,
            borderRadius: token.borderRadius,
            boxShadow: `0 4px 14px ${token.colorPrimary}40`,
          }}
        >
          {t("page.home.heroDrawCard")}
        </Button>
        {role == "registered" ? (
          <Button
            size="large"
            icon={<CrownOutlined />}
            style={{
              height: 48,
              padding: "0 24px",
              fontSize: 16,
              borderRadius: token.borderRadius,
              borderColor: token.colorBorder,
            }}
          >
            {t("page.home.heroUpgradePro")}
          </Button>
        ) : (
          <Button
            size="large"
            onClick={() => {
              navigate(WEB_URL.login);
            }}
            icon={<CrownOutlined />}
            style={{
              height: 48,
              padding: "0 24px",
              fontSize: 16,
              borderRadius: token.borderRadius,
              borderColor: token.colorBorder,
            }}
          >
            {t("page.home.heroLoginNow")}
          </Button>
        )}
      </Space>
    </Col>
  );
}
