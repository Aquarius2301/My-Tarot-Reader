import { CopyButton } from "@/components";
import type { GetAiDeepTarotReadingResult } from "@/types";
import {
  getCrossroadsTimeFrameLabelKey,
  getDeepTarotPositionKeys,
  getDeepTarotPositionLabel,
  matchDeepAnswerSections,
  parseAiDeepTarotAnswer,
} from "@/utils";
import { Card, Flex, Tag, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import { AiDeepTarotResultItemLoading } from "./AiDeepTarotResultItemLoading";
import { PositionSectionCard } from "./PositionSectionCard";

const { Title, Text, Paragraph } = Typography;

export interface AiDeepTarotResultItemProps {
  data?: GetAiDeepTarotReadingResult;
  isLoading?: boolean;
}

/**
 * Shows a created deep tarot reading, whichever spread it belongs to: one card
 * per position (a house name, a calendar month, or one aspect of one of the
 * options the user compared).
 */
export function AiDeepTarotResultItem({
  data,
  isLoading = false,
}: AiDeepTarotResultItemProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  if (isLoading || !data) {
    return <AiDeepTarotResultItemLoading />;
  }

  const isCrossroads = data.topic === "crossroads";
  const options = data.options ?? [];
  const positions = getDeepTarotPositionKeys(data.topic, options.length);
  const answer = parseAiDeepTarotAnswer(data.answer);
  const matchedSections = answer
    ? matchDeepAnswerSections(answer, data.cards, positions)
    : [];
  const orientationLabel = (isReversed: boolean) =>
    isReversed ? t("tarot.position.reversed") : t("tarot.position.upright");

  // Falls back to the card name when the reading has no known position for it.
  const positionLabel = (index: number) =>
    getDeepTarotPositionLabel(
      data.topic,
      index,
      positions[index] ?? "",
      data.createdAt,
      t,
      options,
    ) || t(`tarot.meaning.${data.cards[index].cardCode}.name`);

  const buildCopyText = () => {
    if (!answer) return data.title;

    const sections = [data.title];

    if (answer.overview) {
      sections.push(
        `${t("page.aiDeepTarot.result.overview")}\n${answer.overview}`,
      );
    }

    data.cards.forEach((card, index) => {
      const matched = matchedSections[index];

      sections.push(
        [
          `${positionLabel(index)} · ${t(`tarot.meaning.${card.cardCode}.name`)} · ${orientationLabel(card.isReversed)}`,
          ...(matched?.interpretation ? [matched.interpretation] : []),
        ].join("\n"),
      );
    });

    if (answer.overallAdvice) {
      sections.push(
        `${t("page.aiDeepTarot.result.advice")}\n${answer.overallAdvice}`,
      );
    }

    return sections.join("\n\n");
  };

  return (
    <div>
      {isCrossroads && (data.question || options.length > 0) && (
        <Card
          extra={answer && <CopyButton text={buildCopyText()} size="small" />}
          style={{ marginBottom: token.marginLG }}
        >
          {data.question && (
            <Paragraph style={{ margin: 0 }}>
              <Text strong>{t("page.aiDeepTarot.result.yourQuestion")}</Text>{" "}
              {data.question}
            </Paragraph>
          )}
          {options.length > 0 && (
            <Paragraph style={{ margin: 0 }}>
              <Text strong>{t("page.aiDeepTarot.result.yourOptions")}</Text>
              <Flex
                gap={token.marginXS}
                wrap
                style={{ marginTop: token.marginXS }}
              >
                {options.map((option, index) => (
                  <Tag key={`${option}-${index}`} color="purple">
                    {option}
                  </Tag>
                ))}
              </Flex>
            </Paragraph>
          )}
          {data.timeFrame && (
            <Paragraph style={{ margin: token.marginXS + "px 0 0" }}>
              <Text strong>{t("page.aiDeepTarot.result.yourTimeFrame")}</Text>{" "}
              {t(getCrossroadsTimeFrameLabelKey(data.timeFrame))}
            </Paragraph>
          )}
        </Card>
      )}

      {answer ? (
        <Flex vertical gap={token.marginLG}>
          {answer.overview && (
            <Card>
              <Title level={4} style={{ margin: 0 }}>
                {t("page.aiDeepTarot.result.overview")}
              </Title>
              <Paragraph style={{ margin: 0 }}>{answer.overview}</Paragraph>
            </Card>
          )}

          {data.cards.map((card, index) => (
            <PositionSectionCard
              key={`${card.cardCode}-position-${index}`}
              label={positionLabel(index)}
              section={matchedSections[index]}
              cardCode={card.cardCode}
              isReversed={card.isReversed}
            />
          ))}

          {answer.overallAdvice && (
            <Card>
              <Title level={4} style={{ margin: 0 }}>
                {t("page.aiDeepTarot.result.advice")}
              </Title>
              <Paragraph style={{ margin: 0 }}>
                {answer.overallAdvice}
              </Paragraph>
            </Card>
          )}
        </Flex>
      ) : (
        <Text type="secondary">{t("page.aiDeepTarot.result.noAnswer")}</Text>
      )}
    </div>
  );
}
