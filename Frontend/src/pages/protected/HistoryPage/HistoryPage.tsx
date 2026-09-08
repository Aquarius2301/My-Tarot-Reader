import { Tabs, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import AITarotHistoryTab from "./components/AITarotHistoryTab";
import TarotHistoryTab from "./components/TarotHistoryTab";

const { Title, Text } = Typography;

/** Container page that hosts both the static and AI tarot read-history tabs. */
export default function HistoryPage() {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  return (
    <div
      style={{
        maxWidth: 1080,
        margin: "0 auto",
        padding: `${token.paddingLG}px ${token.paddingMD}px`,
      }}
    >
      {/* Header Section */}
      <div style={{ textAlign: "center", marginBottom: token.marginXL }}>
        <Title level={2} style={{ margin: 0 }}>
          {t("page.history.title")}
        </Title>
        <Text type="secondary">{t("page.history.subtitle")}</Text>
      </div>

      {/* Tab switcher between static and AI read histories */}
      <Tabs
        centered
        items={[
          {
            key: "tarot",
            label: t("page.history.tabTarot"),
            children: <TarotHistoryTab />,
          },
          {
            key: "ai",
            label: t("page.history.tabAi"),
            children: <AITarotHistoryTab />,
          },
        ]}
      />
    </div>
  );
}