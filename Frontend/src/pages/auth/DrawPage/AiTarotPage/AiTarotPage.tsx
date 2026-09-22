import type { AiTarotCardCount, AiTarotQuestionType } from "@/constants";
import { DrawCardsStep, SelectReadingStep } from "./components";
import { Typography } from "antd";
import { useState } from "react";
import { useTranslation } from "react-i18next";

const { Title } = Typography;

interface AiTarotConfig {
  cardCount: AiTarotCardCount;
  questionType: AiTarotQuestionType;
}

/** Two-step AI tarot reading: configure the spread, then draw the cards. */
export default function AiTarotPage() {
  const { t } = useTranslation();
  const [config, setConfig] = useState<AiTarotConfig | null>(null);

  return (
    <div style={{ maxWidth: 960, margin: "0 auto" }}>
      <Title level={2} style={{ textAlign: "center" }}>
        {t("page.aiTarot.title")}
      </Title>

      {config === null ? (
        <SelectReadingStep
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