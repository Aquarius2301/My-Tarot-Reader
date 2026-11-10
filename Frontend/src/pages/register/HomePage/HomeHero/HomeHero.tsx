import { WEB_URL } from "@/routes";
import { Button, Card, Flex, Grid, Space, theme } from "antd";
import { ReadOutlined } from "@ant-design/icons";
import Text from "antd/es/typography/Text";
import Title from "antd/es/typography/Title";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";

export function HomeHero() {
  const { token } = theme.useToken();
  const { t } = useTranslation();
  const navigate = useNavigate();

  const screens = Grid.useBreakpoint();
  const isMobile = screens.lg === false;

  return (
    <Card style={{ marginBottom: 16, padding: isMobile ? 16 : 32 }}>
      <Flex vertical align={isMobile ? "start" : "center"}>
        <Title
          level={isMobile ? 2 : 1}
          style={{
            marginBottom: 8,
          }}
        >
          {t("page.home.heroTitle")}
        </Title>
        <Title
          level={3}
          style={{
            color: token.colorPrimary,
            marginTop: 0,
          }}
        >
          {t("page.home.heroTitleHighlight")}
        </Title>
        <Text type="secondary">{t("page.home.heroDescription")}</Text>
        <Text
          type="secondary"
          style={{
            marginBottom: 16,
          }}
        >
          {t("page.home.heroDescription2")}
        </Text>

        <Space size="middle" wrap>
          <Button
            onClick={() => {
              navigate(WEB_URL.tarot);
            }}
            type="primary"
            size="large"
            icon={<ReadOutlined />}
          >
            {t("page.home.heroDrawCard")}
          </Button>
        </Space>
      </Flex>
    </Card>
  );
}
