import { useLanguageStore } from "@/hooks/store";
import { Card, Flex, Grid, Tag, Typography } from "antd";
import { useTranslation } from "react-i18next";
import type { AiTarotCardCount, AiTarotQuestionType } from "@/types";
import { CardCountSelect, QuestionTypeSelect } from "@/components/select/";

const { Text } = Typography;
const { useBreakpoint } = Grid;

interface AIDrawTarotConfigProps {
  cardCount: AiTarotCardCount;
  questionType: AiTarotQuestionType;
  onCardCountChange: (cardCount: AiTarotCardCount) => void;
  onQuestionTypeChange: (questionType: AiTarotQuestionType) => void;
}

/** Configuration card for an AI reading: card count, question type, and language. */
export default function AIDrawTarotConfig({
  cardCount,
  questionType,
  onCardCountChange,
  onQuestionTypeChange,
}: AIDrawTarotConfigProps) {
  const { t } = useTranslation();
  const screens = useBreakpoint();
  const isMobile = !screens.md;

  const language = useLanguageStore((s) => s.mode);
  const positions = t(`page.aiDraw.cardPositions.${cardCount}`).split(", ");

  return (
    <Card style={{ marginBottom: 24 }}>
      <Flex
        gap={isMobile ? 12 : 24}
        wrap
        vertical={isMobile}
        align={isMobile ? "stretch" : "flex-end"}
      >
        <CardCountSelect
          cardCount={cardCount}
          onCardCountChange={onCardCountChange}
        />

        <QuestionTypeSelect
          questionType={questionType}
          onQuestionTypeChange={onQuestionTypeChange}
        />

        <Flex vertical gap={4} style={{ flex: 1, minWidth: 180 }}>
          <Text strong>{t("page.aiDraw.languageLabel")}</Text>
          {/* The answer language follows the app language, switched via the
              layout's language toggle. */}
          <Text style={{ padding: "4px 0" }}>
            {t(`page.aiDraw.language.${language}`)}
          </Text>
        </Flex>
      </Flex>

      {/* Describe the spread positions for the currently selected card count. */}
      <div style={{ marginTop: 16 }}>
        <Text type="secondary" style={{ display: "block", marginBottom: 8 }}>
          {t("page.aiDraw.cardPositionsLabel")}
        </Text>
        <Flex gap={8} wrap>
          {positions.map((position, index) => (
            <Tag key={position} variant="filled">
              {`${index + 1}. ${position}`}
            </Tag>
          ))}
        </Flex>
      </div>
    </Card>
  );
}
