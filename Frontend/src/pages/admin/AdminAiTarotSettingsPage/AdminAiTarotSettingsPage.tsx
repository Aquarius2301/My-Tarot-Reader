import { ErrorComponent } from "@/components";
import { useGetAdminAiTarotSettings } from "@/hooks/api";
import { Spin, Tabs, Typography } from "antd";
import { useTranslation } from "react-i18next";
import ReadingCountsTab from "./ReadingCountsTab";
import ReadingTypesTab from "./ReadingTypesTab";

const { Title, Text } = Typography;

/**
 * Admin AI tarot settings: manages the available spreads and question topics,
 * including disabled entries.
 */
export default function AdminAiTarotSettingsPage() {
  const { t } = useTranslation();
  const { data, isLoading, refetch } = useGetAdminAiTarotSettings();

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (data === undefined) {
    return <ErrorComponent type="server" onRetry={refetch} />;
  }

  return (
    <div style={{ maxWidth: 1080, margin: "0 auto" }}>
      <div style={{ marginBottom: 24 }}>
        <Title level={2} style={{ marginBottom: 4 }}>
          {t("page.admin.aiTarotSettings.title")}
        </Title>
        <Text type="secondary">
          {t("page.admin.aiTarotSettings.subtitle")}
        </Text>
      </div>

      <Tabs
        defaultActiveKey="readingCounts"
        items={[
          {
            key: "readingCounts",
            label: t("page.admin.aiTarotSettings.tabs.readingCounts"),
            children: <ReadingCountsTab items={data.readingCounts} />,
          },
          {
            key: "readingTypes",
            label: t("page.admin.aiTarotSettings.tabs.readingTypes"),
            children: <ReadingTypesTab items={data.readingTypes} />,
          },
        ]}
      />
    </div>
  );
}
