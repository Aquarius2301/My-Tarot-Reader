import { ErrorComponent } from "@/components";
import { useGetAiTarotSettings } from "@/hooks/api";
import { DrawCardsStep, SelectReadingStep } from "./components";
import { Spin, Typography } from "antd";
import { useState } from "react";
import { useTranslation } from "react-i18next";

const { Title } = Typography;

interface AiTarotConfig {
  cardCount: number;
  questionType: string;
}

/** Two-step AI tarot reading: configure the spread, then draw the cards. */
export default function AiTarotPage() {
  const { t } = useTranslation();
  const { data: settings, isLoading, refetch } = useGetAiTarotSettings();
  const [config, setConfig] = useState<AiTarotConfig | null>(null);

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (settings === undefined) {
    return <ErrorComponent type="server" onRetry={refetch} />;
  }

  return (
    <div style={{ maxWidth: 960, margin: "0 auto" }}>
      <Title level={2} style={{ textAlign: "center" }}>
        {t("page.aiTarot.title")}
      </Title>

      {config === null ? (
        <SelectReadingStep
          settings={settings}
          onNext={(cardCount, questionType) =>
            setConfig({ cardCount, questionType })
          }
        />
      ) : (
        <DrawCardsStep
          cardCount={config.cardCount}
          questionType={config.questionType}
          onBack={() => setConfig(null)}
        />
      )}
    </div>
  );
}
