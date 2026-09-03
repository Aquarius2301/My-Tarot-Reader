import { TarotDeck, type SpreadResultItem } from "@/components";
import { useCreateAiTarotReading } from "@/hooks/api";
import { useLanguageStore } from "@/hooks/store";
import { getErrorMessage } from "@/utils";
import { App, Grid, Spin, Typography } from "antd";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type {
  AiTarotCardCount,
  AiTarotQuestionType,
  CreateAiTarotReadingRequest,
  CreateAiTarotReadingResponse,
} from "@/types";
import { AIDrawTarotConfig, AIDrawTarotResult } from "./components";
import { CARD_COUNT_LIMIT, type LanguageMode } from "@/constants";

const { Title, Text } = Typography;
const { useBreakpoint } = Grid;

/** The drawn cards and AI answer produced by a completed reading. */
export interface AiReadingResult {
  response: CreateAiTarotReadingResponse;
  cards: SpreadResultItem[];
}

export default function AIDrawTarotPage() {
  const { t } = useTranslation();
  const { message } = App.useApp();
  const screens = useBreakpoint();
  const isMobile = !screens.md;

  const language = useLanguageStore((s) => s.mode);
  const { mutate, isPending } = useCreateAiTarotReading();

  const [cardCount, setCardCount] = useState<AiTarotCardCount>("three");
  const [questionType, setQuestionType] =
    useState<AiTarotQuestionType>("energy");
  const [result, setResult] = useState<AiReadingResult | null>(null);
  // Bumped to force a fresh TarotDeck (and clear its selection) after each reading.
  const [deckKey, setDeckKey] = useState(0);

  const handleGetReading = (selectedCards: SpreadResultItem[]) => {
    const payload: CreateAiTarotReadingRequest = {
      cardCount,
      questionType,
      cards: selectedCards.map((card) => ({
        code: card.cardCode,
        isReversed: card.isReversed,
      })),
      language: language as LanguageMode,
    };

    mutate(payload, {
      onSuccess: (response) => {
        setResult({ response, cards: selectedCards });
        setDeckKey((key) => key + 1);
      },
      onError: (error) => {
        message.error(getErrorMessage(error));
      },
    });
  };

  const handleDrawAgain = () => {
    setResult(null);
    setDeckKey((key) => key + 1);
  };

  if (result) {
    return <AIDrawTarotResult result={result} onDrawAgain={handleDrawAgain} />;
  }

  return (
    <div style={{ maxWidth: 960, margin: "0 auto" }}>
      <Title level={3} style={{ textAlign: "center" }}>
        {t("page.aiDraw.title")}
      </Title>
      <Text
        type="secondary"
        style={{ display: "block", textAlign: "center", marginBottom: 20 }}
      >
        {t("page.aiDraw.intro")}
      </Text>

      <AIDrawTarotConfig
        cardCount={cardCount}
        questionType={questionType}
        onCardCountChange={setCardCount}
        onQuestionTypeChange={setQuestionType}
      />

      <TarotDeck
        key={`${cardCount}-${deckKey}`}
        limit={CARD_COUNT_LIMIT[cardCount]}
        cardSize={isMobile ? "sm" : "md"}
        onConfirm={handleGetReading}
      />

      {isPending && <Spin fullscreen description={t("page.aiDraw.saving")} />}
    </div>
  );
}
