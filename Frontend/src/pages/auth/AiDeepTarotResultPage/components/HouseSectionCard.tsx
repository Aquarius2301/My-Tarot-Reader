import { TarotCard } from "@/components";
import type { TarotCardCode } from "@/constants";
import type { AiDeepTarotAnswerSection } from "@/types";
import { Card, Flex, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";

const { Text, Paragraph } = Typography;

export interface HouseSectionCardProps {
  positionKey: string;
  section?: AiDeepTarotAnswerSection;
  cardCode: TarotCardCode;
  isReversed: boolean;
}

/** One house of the spread: the drawn card plus its AI interpretation. */
export default function HouseSectionCard({
  positionKey,
  section,
  cardCode,
  isReversed,
}: HouseSectionCardProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const cardName = t(`tarot.meaning.${cardCode}.name`);
  const orientation = isReversed
    ? t("tarot.position.reversed")
    : t("tarot.position.upright");

  return (
    <Card>
      <Flex align="center" gap={token.marginSM} wrap>
        <TarotCard cardCode={cardCode} isUpright={!isReversed} isFlipped size="sm" />
        <div>
          <Text strong style={{ display: "block" }}>
            {t(`page.aiDeepTarot.house.${positionKey}`)}
          </Text>
          {section?.title && (
            <Text type="secondary" style={{ display: "block" }}>
              {section.title}
            </Text>
          )}
          <Text type="secondary">
            {cardName} · {orientation}
          </Text>
        </div>
      </Flex>

      {section?.interpretation && (
        <Paragraph style={{ marginTop: token.marginSM, marginBottom: 0 }}>
          {section.interpretation}
        </Paragraph>
      )}
    </Card>
  );
}
