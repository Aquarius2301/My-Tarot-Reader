import { Spin, Typography } from "antd";
import { useTranslation } from "react-i18next";
import { ErrorComponent } from "@/components";
import { useGetTarotStatistic } from "@/hooks/api";
import { StatCards, AiTarotBreakdownTable } from "./components";

const { Title, Text } = Typography;

/**
 * Admin statistics page: total guest / registered / AI tarot readings and
 * the AI readings broken down by question type and card count.
 */
export default function AdminStatsPage() {
  const { t } = useTranslation();
  const { data, isLoading, refetch } = useGetTarotStatistic();

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (data === undefined) {
    return <ErrorComponent type="server" onRetry={refetch} />;
  }

  return (
    <div style={{ maxWidth: 960, margin: "0 auto" }}>
      <Title level={2} style={{ textAlign: "center", marginBottom: 4 }}>
        {t("page.admin.statistics.title")}
      </Title>
      <Text
        type="secondary"
        style={{ display: "block", textAlign: "center", marginBottom: 24 }}
      >
        {t("page.admin.statistics.subtitle")}
      </Text>

      <StatCards
        totalGuest={data.totalGuestTarotReadings}
        totalRegistered={data.totalRegisteredTarotReadings}
        totalAi={data.totalAiTarotReadings}
      />

      <div style={{ marginTop: 24 }}>
        <AiTarotBreakdownTable items={data.aiTarotBreakdown} />
      </div>
    </div>
  );
}
