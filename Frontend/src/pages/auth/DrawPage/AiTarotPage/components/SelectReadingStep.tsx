import {
  AI_TAROT_CARD_COUNTS,
  AI_TAROT_COSTS,
  AI_TAROT_POSITIONS,
  AI_TAROT_QUESTION_TYPES,
  type AiTarotCardCount,
  type AiTarotQuestionType,
} from "@/constants";
import { useGetCurrentUser } from "@/hooks/api";
import { Button, Card, Flex, Segmented, Typography, theme } from "antd";
import { ThunderboltFilled, WalletFilled } from "@ant-design/icons";
import { useState } from "react";
import { useTranslation } from "react-i18next";

const { Text } = Typography;

export interface SelectReadingStepProps {
  onNext: (
    cardCount: AiTarotCardCount,
    questionType: AiTarotQuestionType,
  ) => void;
}

/** Step 1: pick the spread size and question topic before drawing cards. */
export default function SelectReadingStep({
  onNext,
}: SelectReadingStepProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const { data: user } = useGetCurrentUser();

  const [cardCount, setCardCount] = useState<AiTarotCardCount>(3);
  const [questionType, setQuestionType] =
    useState<AiTarotQuestionType>("energy");

  const cost = AI_TAROT_COSTS[cardCount];
  const balance = user?.whiteCoin ?? 0;
  const canAfford = balance >= cost;
  const positions = AI_TAROT_POSITIONS[cardCount];

  return (
    <Flex vertical gap={token.marginLG}>
      <Card>
        <Flex vertical gap={token.marginMD}>
          <div>
            <Text strong>{t("page.aiTarot.cardCount")}</Text>
            <Segmented
              block
              value={cardCount}
              onChange={(value) => setCardCount(value as AiTarotCardCount)}
              options={AI_TAROT_CARD_COUNTS.map((count) => ({
                label: t("page.aiTarot.cardCountOption", { count }),
                value: count,
              }))}
              style={{ marginTop: token.marginSM }}
            />
          </div>

          <div>
            <Text strong>{t("page.aiTarot.positions")}</Text>
            <Flex
              vertical
              gap={token.marginXS}
              style={{ marginTop: token.marginSM }}
            >
              {positions.map((positionKey, index) => (
                <Flex key={positionKey} gap={token.marginSM} align="baseline">
                  <Text type="secondary">{index + 1}.</Text>
                  <Text>{t(`page.aiTarot.position.${positionKey}`)}</Text>
                </Flex>
              ))}
            </Flex>
          </div>
        </Flex>
      </Card>

      <Card>
        <Text strong>{t("page.aiTarot.questionType")}</Text>
        <Segmented
          block
          value={questionType}
          onChange={(value) => setQuestionType(value as AiTarotQuestionType)}
          options={AI_TAROT_QUESTION_TYPES.map((type) => ({
            label: t(`page.aiTarot.questionTypes.${type}`),
            value: type,
          }))}
          style={{ marginTop: token.marginSM }}
        />
      </Card>

      <Card>
        <Flex vertical gap={token.marginSM}>
          <Flex justify="space-between" align="center">
            <Text>
              <WalletFilled /> {t("page.aiTarot.balance", { balance })}
            </Text>
            <Text>
              <ThunderboltFilled /> {t("page.aiTarot.cost", { cost })}
            </Text>
          </Flex>

          {!canAfford && (
            <Text type="warning">
              {t("page.aiTarot.insufficientCoins", { cost, balance })}
            </Text>
          )}

          <Button
            type="primary"
            size="large"
            block
            disabled={!canAfford}
            onClick={() => onNext(cardCount, questionType)}
          >
            {t("page.aiTarot.continue")}
          </Button>
        </Flex>
      </Card>
    </Flex>
  );
}