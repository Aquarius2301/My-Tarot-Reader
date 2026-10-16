import { useGetCurrentUser } from "@/hooks/api";
import { useLanguageStore } from "@/hooks/stores";
import type { GetAiTarotSettingResult } from "@/types";
import { Button, Card, Flex, Segmented, Typography, theme } from "antd";
import { ThunderboltFilled, WalletFilled } from "@ant-design/icons";
import { useState } from "react";
import { useTranslation } from "react-i18next";

const { Text } = Typography;

export interface SelectReadingStepProps {
  settings: GetAiTarotSettingResult;
  onNext: (cardCount: number, questionType: string) => void;
}

/** Step 1: pick the spread size and question topic before drawing cards. */
export default function SelectReadingStep({
  settings,
  onNext,
}: SelectReadingStepProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const { data: user } = useGetCurrentUser();
  const language = useLanguageStore((s) => s.mode);

  const [cardCount, setCardCount] = useState<number>(
    settings.readingCounts[0]?.cardCount ?? 0,
  );
  const [questionType, setQuestionType] = useState<string>(
    settings.readingTypes[0]?.code ?? "",
  );

  const countSetting = settings.readingCounts.find(
    (item) => item.cardCount === cardCount,
  );
  const cost = countSetting?.whiteCoins ?? 0;
  const balance = user?.whiteCoin ?? 0;
  const canAfford = balance >= cost;
  const positions =
    (language === "vi" ? countSetting?.positionsVi : countSetting?.positionsEn) ??
    [];

  return (
    <Flex vertical gap={token.marginLG}>
      <Card>
        <Flex vertical gap={token.marginMD}>
          <div>
            <Text strong>{t("page.aiTarot.cardCount")}</Text>
            <Segmented
              block
              value={cardCount}
              onChange={(value) => setCardCount(value as number)}
              options={settings.readingCounts.map((item) => ({
                label: t("page.aiTarot.cardCountOption", {
                  count: item.cardCount,
                }),
                value: item.cardCount,
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
              {positions.map((position, index) => (
                <Flex key={index} gap={token.marginSM} align="baseline">
                  <Text type="secondary">{index + 1}.</Text>
                  <Text>{position}</Text>
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
          onChange={(value) => setQuestionType(value as string)}
          options={settings.readingTypes.map((type) => ({
            label: language === "vi" ? type.nameVi : type.nameEn,
            value: type.code,
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
