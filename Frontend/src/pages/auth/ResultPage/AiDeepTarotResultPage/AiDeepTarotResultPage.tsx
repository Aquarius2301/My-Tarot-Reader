import { CopyButton, ErrorComponent } from "@/components";
import { AI_DEEP_TAROT_POSITIONS, type AiDeepTarotTopic } from "@/constants";
import { useGetAiDeepTarotReadingById } from "@/hooks/api";
import { WEB_URL } from "@/routes";
import {
  getDeepTarotPositionLabel,
  matchDeepAnswerSections,
  parseAiDeepTarotAnswer,
} from "@/utils";
import { Button, Card, Flex, Spin, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router-dom";
import { PositionSectionCard } from "./components";

const { Title, Text, Paragraph } = Typography;

/** Where to send the user to draw a new reading of the same spread. */
const DRAW_URL_BY_TOPIC = {
  twelveHouses: WEB_URL.aiDeepTarotTwelveHouses,
  twelveMonths: WEB_URL.aiDeepTarotTwelveMonths,
} as const satisfies Record<AiDeepTarotTopic, string>;

/**
 * Shows a created deep tarot reading, whichever spread it belongs to: the
 * title, then one card per position (a house name, or a calendar month).
 */
export default function AiDeepTarotResultPage() {
  const { readingId } = useParams<{ readingId: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const { data, isLoading, refetch } = useGetAiDeepTarotReadingById(
    readingId ?? "",
    !!readingId,
  );

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (!data) {
    return <ErrorComponent type="server" onRetry={refetch} />;
  }

  const positions = AI_DEEP_TAROT_POSITIONS[data.topic] ?? [];
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
    <div
      style={{
        maxWidth: 1080,
        margin: "0 auto",
        padding: `${token.paddingLG}px ${token.paddingMD}px`,
      }}
    >
      <div style={{ textAlign: "center", marginBottom: token.marginXL }}>
        <Title level={2} style={{ margin: 0 }}>
          {data.title}
        </Title>
        <Flex justify="center" align="center" gap={token.marginSM} wrap>
          <Text type="secondary">{t("page.aiDeepTarot.result.title")}</Text>
          {answer && <CopyButton text={buildCopyText()} size="small" />}
        </Flex>
      </div>

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
              <Paragraph style={{ margin: 0 }}>{answer.overallAdvice}</Paragraph>
            </Card>
          )}
        </Flex>
      ) : (
        <Text type="secondary">{t("page.aiDeepTarot.result.noAnswer")}</Text>
      )}

      <Flex justify="center" style={{ marginTop: token.marginXL }}>
        <Button
          type="primary"
          size="large"
          onClick={() => navigate(DRAW_URL_BY_TOPIC[data.topic])}
        >
          {t("page.aiDeepTarot.result.drawAgain")}
        </Button>
      </Flex>
    </div>
  );
}
