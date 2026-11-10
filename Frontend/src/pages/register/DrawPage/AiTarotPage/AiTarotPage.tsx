import { useGetAiTarotSettings } from "@/hooks/api";
import {
  DrawCardsStep,
  SelectReadingStep,
  SelectReadingStepLoading,
} from "./components";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { AppTitle, ErrorView } from "@/components";

interface AiTarotConfig {
  cardCount: number;
  questionType: string;
}

/** Two-step AI tarot reading: configure the spread, then draw the cards. */
export default function AiTarotPage() {
  const { t } = useTranslation();
  const { data: settings, isLoading, refetch } = useGetAiTarotSettings();
  const [config, setConfig] = useState<AiTarotConfig | null>(null);

  return (
    <div>
      <AppTitle
        title={t("page.aiTarot.title")}
        subtitle={t("page.aiTarot.subTitle")}
      />

      {isLoading ? (
        <SelectReadingStepLoading />
      ) : settings === undefined ? (
        <ErrorView type="server" onRetry={refetch} />
      ) : config === null ? (
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
