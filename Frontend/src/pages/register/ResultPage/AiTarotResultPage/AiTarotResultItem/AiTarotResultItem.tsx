import { CopyButton, ErrorView, TarotCard } from "@/components";
import { useGetAiTarotReadingById } from "@/hooks/api";
import { matchAnswerCards, parseAiTarotAnswer } from "@/utils";
import { Card, Flex, Space, theme } from "antd";
import Paragraph from "antd/es/typography/Paragraph";
import Text from "antd/es/typography/Text";
import Title from "antd/es/typography/Title";
import { useTranslation } from "react-i18next";
import { useParams } from "react-router-dom";
import { AiTarotResultItemLoading } from "./AiTarotResultItemLoading";

export function AiTarotResultItem() {
  const { token } = theme.useToken();
  const { t } = useTranslation();
  const { readingId } = useParams<{ readingId: string }>();

  const { data, isLoading, refetch } = useGetAiTarotReadingById(
    readingId ?? "",
    !!readingId,
  );

  if (isLoading) {
    return <AiTarotResultItemLoading />;
  }

  if (!data) {
    return <ErrorView type="server" onRetry={refetch} />;
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
        <Card
          title={
            <Title level={4} style={{ margin: 0 }}>
              {data.title}
            </Title>
          }
          extra={answer && <CopyButton text={buildCopyText()} size="small" />}
        >
          <Title level={4} style={{ margin: 0 }}>
            {t("page.aiTarot.result.overview")}
          </Title>
          <Paragraph style={{ margin: 0 }}>{answer.overview}</Paragraph>
        </Card>
      )}

      {/* Cards section */}
      {data.cards.map((card, index) => {
        const answerCard = matchedAnswerCards[index];
        const positionLabel = answerCard?.position;
        const name = t(`tarot.meaning.${card.cardCode}.name`);
        const orientation = orientationLabel(card.isReversed);

        return (
          <Card key={`${card.cardCode}-meaning-${index}`}>
            <Flex align="center" gap={token.marginSM} wrap>
              <TarotCard
                cardCode={card.cardCode}
                isUpright={!card.isReversed}
                isFlipped
                size="sm"
              />
              <Space vertical>
                <Text strong>{positionLabel || name}</Text>
                <Text type="secondary">
                  {name} · {orientation}
                </Text>
              </Space>
            </Flex>
            {answerCard?.interpretation && (
              <Paragraph style={{ marginTop: token.marginSM }}>
                {answerCard.interpretation}
              </Paragraph>
            )}
          </Card>
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
