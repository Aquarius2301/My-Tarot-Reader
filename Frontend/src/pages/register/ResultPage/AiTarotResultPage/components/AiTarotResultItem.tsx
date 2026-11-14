import { CopyButton } from "@/components";
import type { GetAiTarotReadingResult } from "@/types";
import { matchAnswerCards, parseAiTarotAnswer } from "@/utils";
import { Card, Flex, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import { InterpretationCard } from "./InterpretationCard";
import { AiTarotResultItemLoading } from "./AiTarotResultItemLoading";

const { Title, Text, Paragraph } = Typography;

interface AiTarotResultItemProps {
  data?: GetAiTarotReadingResult;
  isLoading?: boolean;
}

/** Overview, per-card interpretations and overall advice of an AI reading. */
export function AiTarotResultItem({
  data,
  isLoading = false,
}: AiTarotResultItemProps) {
  const { token } = theme.useToken();
  const { t } = useTranslation();

  if (isLoading || !data) {
    return <AiTarotResultItemLoading />;
  }

  const answer = parseAiTarotAnswer(data.answer);
  const matchedAnswerCards = answer ? matchAnswerCards(answer, data.cards) : [];
  const orientationLabel = (isReversed: boolean) =>
    isReversed ? t("tarot.position.reversed") : t("tarot.position.upright");

  const buildCopyText = () => {
    if (!answer) return data.title;

    const sections = [data.title];

    if (answer.overview) {
      sections.push(`${t("page.aiTarot.result.overview")}\n${answer.overview}`);
    }

    data.cards.forEach((card, index) => {
      const answerCard = matchedAnswerCards[index];
      const positionLabel = answerCard?.position;
      const header = positionLabel || t(`tarot.meaning.${card.cardCode}.name`);

      sections.push(
        [
          `${header} · ${t(`tarot.meaning.${card.cardCode}.name`)} · ${orientationLabel(card.isReversed)}`,
          ...(answerCard?.interpretation ? [answerCard.interpretation] : []),
        ].join("\n"),
      );
    });

    if (answer.overallAdvice) {
      sections.push(
        `${t("page.aiTarot.result.advice")}\n${answer.overallAdvice}`,
      );
    }

    return sections.join("\n\n");
  };

  return answer ? (
    <Flex vertical gap={token.marginLG}>
      {/* Overview section */}
      {answer.overview && (
        <Card extra={<CopyButton text={buildCopyText()} size="small" />}>
          <Title level={4} style={{ margin: 0 }}>
            {t("page.aiTarot.result.overview")}
          </Title>
          <Paragraph style={{ margin: 0 }}>{answer.overview}</Paragraph>
        </Card>
      )}

      {/* Cards section */}
      {data.cards.map((card, index) => {
        const answerCard = matchedAnswerCards[index];

        return (
          <InterpretationCard
            key={`${card.cardCode}-meaning-${index}`}
            position={answerCard?.position}
            interpretation={answerCard?.interpretation}
            cardCode={card.cardCode}
            isReversed={card.isReversed}
          />
        );
      })}

      {/* Overall advice section */}
      {answer.overallAdvice && (
        <Card>
          <Title level={4} style={{ margin: 0 }}>
            {t("page.aiTarot.result.advice")}
          </Title>
          <Paragraph style={{ margin: 0 }}>{answer.overallAdvice}</Paragraph>
        </Card>
      )}
    </Flex>
  ) : (
    <Text type="secondary">{t("page.aiTarot.result.noAnswer")}</Text>
  );
}
