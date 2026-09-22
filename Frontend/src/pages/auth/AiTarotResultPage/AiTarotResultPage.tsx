import { ErrorComponent, TarotCard } from "@/components";
import { AI_TAROT_CARD_COUNT_BY_VALUE, AI_TAROT_POSITIONS } from "@/constants";
import { useGetAiTarotReadingById } from "@/hooks/api";
import { WEB_URL } from "@/routes";
import { matchAnswerCards, parseAiTarotAnswer } from "@/utils";
import { Button, Card, Flex, Spin, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router-dom";

const { Title, Text, Paragraph } = Typography;

/** Shows a created AI tarot reading: title, drawn cards and its meanings. */
export default function AiTarotResultPage() {
  const { readingId } = useParams<{ readingId: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const { data, isLoading, refetch } = useGetAiTarotReadingById(
    readingId ?? "",
    !!readingId,
  );

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (!data) {
    return <ErrorComponent type="server" onRetry={refetch} />;
  }

  const answer = parseAiTarotAnswer(data.answer);
  const cardCount = AI_TAROT_CARD_COUNT_BY_VALUE[data.cardCount];
  const positions = AI_TAROT_POSITIONS[cardCount];
  const matchedAnswerCards = answer ? matchAnswerCards(answer, data.cards) : [];
  const orientationLabel = (isReversed: boolean) =>
    isReversed ? t("tarot.position.reversed") : t("tarot.position.upright");

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
        <Text type="secondary">{t("page.aiTarot.result.title")}</Text>
      </div>

      {/* Drawn cards */}
      {/* <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(auto-fill, minmax(200px, 1fr))",
          gap: token.marginLG,
          marginBottom: token.marginXL,
        }}
      >
        {data.cards.map((card, index) => {
          const positionKey = positions[index];
          return (
            <div
              key={`${card.cardCode}-${index}`}
              style={{
                textAlign: "center",
                display: "flex",
                flexDirection: "column",
                alignItems: "center",
                gap: token.marginSM,
                padding: 0,
              }}
              // styles={{
              //   body: {

              //   },
              // }}
            >
              <TarotCard
                cardCode={card.cardCode}
                isUpright={!card.isReversed}
                isFlipped
                size="md"
              />
              <div>
                <Text strong style={{ display: "block" }}>
                  {t(`tarot.meaning.${card.cardCode}.name`)} ·{" "}
                  {orientationLabel(card.isReversed)}
                </Text>
                {positionKey && (
                  <Text type="secondary">
                    {t(`page.aiTarot.position.${positionKey}`)}
                  </Text>
                )}
              </div>
            </div>
          );
        })}
      </div> */}

      {answer ? (
        <Flex vertical gap={token.marginLG}>
          {answer.overview && (
            <Card>
              <Title level={4} style={{ margin: 0 }}>
                {t("page.aiTarot.result.overview")}
              </Title>
              <Paragraph style={{ margin: 0 }}>{answer.overview}</Paragraph>
            </Card>
          )}

          {data.cards.map((card, index) => {
            const answerCard = matchedAnswerCards[index];
            const positionKey = positions[index];
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
                  <div>
                    <Text strong style={{ display: "block" }}>
                      {positionKey
                        ? t(`page.aiTarot.position.${positionKey}`)
                        : name}
                    </Text>
                    <Text type="secondary">
                      {name} · {orientation}
                    </Text>
                  </div>
                </Flex>
                {answerCard?.interpretation && (
                  <Paragraph style={{ marginTop: token.marginSM }}>
                    {answerCard.interpretation}
                  </Paragraph>
                )}
              </Card>
            );
          })}

          {answer.overallAdvice && (
            <Card>
              <Title level={4} style={{ margin: 0 }}>
                {t("page.aiTarot.result.advice")}
              </Title>
              <Paragraph style={{ margin: 0 }}>
                {answer.overallAdvice}
              </Paragraph>
            </Card>
          )}
        </Flex>
      ) : (
        <Text type="secondary">{t("page.aiTarot.result.noAnswer")}</Text>
      )}

      <Flex justify="center" style={{ marginTop: token.marginXL }}>
        <Button
          type="primary"
          size="large"
          onClick={() => navigate(WEB_URL.aiTarot)}
        >
          {t("page.aiTarot.result.drawAgain")}
        </Button>
      </Flex>
    </div>
  );
}
