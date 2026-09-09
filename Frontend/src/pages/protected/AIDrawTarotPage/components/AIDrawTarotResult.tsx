import { AiMarkdown, DrawnCardRow } from "@/components";
import { Button, Card, Typography } from "antd";
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

      <DrawnCardRow cards={result.cards} size="sm" />

      <Card style={{ textAlign: "left" }}>
        <AiMarkdown content={result.response.answer} />
      </Card>

      <Button type="primary" style={{ marginTop: 24 }} onClick={onDrawAgain}>
        {t("page.aiDraw.drawAgain")}
      </Button>
    </div>
  );
}
