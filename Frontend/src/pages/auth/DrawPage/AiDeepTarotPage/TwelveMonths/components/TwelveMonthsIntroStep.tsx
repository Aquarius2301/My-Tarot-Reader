import { AI_DEEP_TAROT_CARD_COUNTS, AI_DEEP_TAROT_COSTS } from "@/constants";
import { useGetCurrentUser } from "@/hooks/api";
import { Card, Button, Flex, Typography, theme } from "antd";
import { ThunderboltFilled, WalletFilled } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import TwelveMonthsGuide from "./TwelveMonthsGuide";

const { Text } = Typography;

export interface TwelveMonthsIntroStepProps {
  onNext: () => void;
}

/** Step 1: explain the 12 months spread, then check the red coin balance. */
export default function TwelveMonthsIntroStep({
  onNext,
}: TwelveMonthsIntroStepProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const { data: user } = useGetCurrentUser();

  const cardCount = AI_DEEP_TAROT_CARD_COUNTS.twelveMonths;
  const cost = AI_DEEP_TAROT_COSTS.twelveMonths;
  const balance = user?.redCoin ?? 0;
  const canAfford = balance >= cost;

  return (
    <Flex vertical gap={token.marginLG}>
      <Text
        type="secondary"
        style={{ display: "block", textAlign: "center" }}
      >
        {t("page.aiDeepTarot.spreads.twelveMonths.subtitle")}
      </Text>

      <TwelveMonthsGuide />

      <Card>
        <Flex vertical gap={token.marginSM}>
          <Flex justify="space-between" align="center" wrap>
            <Text>
              <WalletFilled />{" "}
              {t("page.aiDeepTarot.common.balance", { balance })}
            </Text>
            <Text>
              <ThunderboltFilled />{" "}
              {t("page.aiDeepTarot.common.cost", { cost })}
            </Text>
          </Flex>

          <Text type="secondary">
            {t("page.aiDeepTarot.common.cardCount", {
              count: cardCount,
              positions: cardCount,
            })}
          </Text>

          {!canAfford && (
            <Text type="warning">
              {t("page.aiDeepTarot.common.insufficientCoins", { cost, balance })}
            </Text>
          )}

          <Button
            type="primary"
            size="large"
            block
            disabled={!canAfford}
            onClick={onNext}
          >
            {t("page.aiDeepTarot.common.continue")}
          </Button>
        </Flex>
      </Card>
    </Flex>
  );
}
