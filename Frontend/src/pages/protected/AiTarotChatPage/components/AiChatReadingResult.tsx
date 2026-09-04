import { TarotCard, type SpreadResultItem } from "@/components";
import { Button, Card, Typography } from "antd";
import ReactMarkdown from "react-markdown";
import { useTranslation } from "react-i18next";

const { Title, Text } = Typography;

interface AiChatReadingResultProps {
  /** The AI-generated interpretation text. */
  answer: string;
  /** The drawn cards displayed in the reading. */
  cards: SpreadResultItem[];
  /** Callback when the user clicks "Start New Chat". */
  onNewChat: () => void;
}

/**
 * Displays the final reading result: drawn cards in a row followed by
 * the AI markdown interpretation and a "New Chat" button.
 */
export default function AiChatReadingResult({
  answer,
  cards,
  onNewChat,
}: AiChatReadingResultProps) {
  const { t } = useTranslation();

  return (
    <div style={{ maxWidth: 720, margin: "0 auto", textAlign: "center" }}>
      <Title level={3}>{t("page.aiChat.readingTitle")}</Title>

      <Text strong style={{ display: "block", marginBottom: 12 }}>
        {t("page.aiChat.readingYourCards")}
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
        {cards.map((card) => (
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
          {answer}
        </ReactMarkdown>
      </Card>

      <Button type="primary" style={{ marginTop: 24 }} onClick={onNewChat}>
        {t("page.aiChat.newChat")}
      </Button>
    </div>
  );
}
