import { AiMarkdown, DrawnCardRow, type SpreadResultItem } from "@/components";
import { Button, Card, Typography } from "antd";
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

      <DrawnCardRow cards={cards} size="sm" />

      <Card style={{ textAlign: "left" }}>
        <AiMarkdown content={answer} />
      </Card>

      <Button type="primary" style={{ marginTop: 24 }} onClick={onNewChat}>
        {t("page.aiChat.newChat")}
      </Button>
    </div>
  );
}
