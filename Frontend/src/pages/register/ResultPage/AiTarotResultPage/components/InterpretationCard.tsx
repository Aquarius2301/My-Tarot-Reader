import { TarotCard } from "@/components";
import type { TarotCardCode } from "@/constants";
import { Card, Flex, Space, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";

const { Text, Paragraph } = Typography;

interface InterpretationCardProps {
  position?: string;
  interpretation?: string;
  cardCode: TarotCardCode;
  isReversed: boolean;
}

/** One drawn card of the reading plus its AI interpretation. */
export function InterpretationCard({
  position,
  interpretation,
  cardCode,
  isReversed,
}: InterpretationCardProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const cardName = t(`tarot.meaning.${cardCode}.name`);
  const orientation = isReversed
    ? t("tarot.position.reversed")
    : t("tarot.position.upright");

  return (
    <Card>
      <Flex align="center" gap={token.marginSM} wrap>
        <TarotCard
          cardCode={cardCode}
          isUpright={!isReversed}
          isFlipped
          size="sm"
        />
        <Space vertical>
          <Text strong>{position || cardName}</Text>
          <Text type="secondary">
            {cardName} · {orientation}
          </Text>
        </Space>
      </Flex>
      {interpretation && (
        <Paragraph style={{ marginTop: token.marginSM }}>
          {interpretation}
        </Paragraph>
      )}
    </Card>
  );
}
