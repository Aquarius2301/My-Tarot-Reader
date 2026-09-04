import { TarotCard } from "@/components";
import { Button, Card, Typography } from "antd";
import ReactMarkdown from "react-markdown";
import { useTranslation } from "react-i18next";
import type { AiReadingResult } from "../AIDrawTarotPage";

const { Title, Text } = Typography;

interface AIDrawTarotResultProps {
  result: AiReadingResult;
  onDrawAgain: () => void;
}

/** Displays the drawn cards and the AI markdown interpretation for a reading. */
export default function AIDrawTarotResult({
  result,
  onDrawAgain,
}: AIDrawTarotResultProps) {
  const { t } = useTranslation();

  return (
    <div style={{ maxWidth: 720, margin: "0 auto", textAlign: "center" }}>
      <Title level={3}>{t("page.aiDraw.resultTitle")}</Title>

      <Text strong style={{ display: "block", marginBottom: 12 }}>
        {t("page.aiDraw.yourCards")}
      </Text>
      <div
        style={{
          display: "flex",
          flexWrap: "wrap",
          justifyContent: "center",
          gap: 12,
          marginBottom: 24,
        }}
      >
        {result.cards.map((card) => (
          <TarotCard
            key={card.cardCode}
            cardCode={card.cardCode}
            isUpright={!card.isReversed}
            isFlipped
            size="sm"
          />
        ))}
      </div>

      <Card style={{ textAlign: "left" }}>
        <ReactMarkdown
          components={{
            h1: ({ children }) => (
              <Typography.Title level={3}>{children}</Typography.Title>
            ),
            h2: ({ children }) => (
              <Typography.Title level={4}>{children}</Typography.Title>
            ),
            h3: ({ children }) => (
              <Typography.Title level={5}>{children}</Typography.Title>
            ),
            h4: ({ children }) => (
              <Typography.Title level={5}>{children}</Typography.Title>
            ),
            p: ({ children }) => (
              <Typography.Paragraph
                style={{ marginBottom: 12, fontSize: 16, lineHeight: 1.7 }}
              >
                {children}
              </Typography.Paragraph>
            ),
            strong: ({ children }) => <Text strong>{children}</Text>,
            li: ({ children }) => (
              <li style={{ marginBottom: 4 }}>{children}</li>
            ),
          }}
        >
          {result.response.answer}
        </ReactMarkdown>
      </Card>

      <Button type="primary" style={{ marginTop: 24 }} onClick={onDrawAgain}>
        {t("page.aiDraw.drawAgain")}
      </Button>
    </div>
  );
}
