import { Coin } from "@/components";
import { useGetCurrentUser } from "@/hooks/api";
import { useLanguageStore } from "@/hooks/stores";
import type { GetAiTarotSettingResult } from "@/types";
import {
  Button,
  Card,
  Col,
  Empty,
  Flex,
  Grid,
  Radio,
  Row,
  Typography,
  theme,
} from "antd";
import { ThunderboltFilled, WalletFilled } from "@ant-design/icons";
import { useState, type CSSProperties } from "react";
import { useTranslation } from "react-i18next";

const { Text } = Typography;

export interface SelectReadingStepProps {
  settings: GetAiTarotSettingResult;
  onNext: (cardCount: number, questionType: string) => void;
}

/** Step 1: pick the spread size and question topic before drawing cards. */
export function SelectReadingStep({
  settings,
  onNext,
}: SelectReadingStepProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
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
    (language === "vi"
      ? countSetting?.positionsVi
      : countSetting?.positionsEn) ?? [];

  const cardSize = isMobile ? "small" : "medium";

  const optionStyle = (isSelected: boolean): CSSProperties => ({
    width: "100%",
    minHeight: 44,
    marginInlineStart: 0,
    alignItems: "center",
    padding: `${token.paddingXS}px ${token.paddingSM}px`,
    borderRadius: token.borderRadiusLG,
    border: `1px solid ${
      isSelected ? token.colorPrimary : token.colorBorderSecondary
    }`,
    background: isSelected ? token.colorPrimaryBg : "transparent",
  });

  return (
    <Flex vertical gap={token.marginLG}>
      <Row gutter={[token.marginMD, token.marginMD]}>
        <Col xs={24} sm={12}>
          <Card size={cardSize} style={{ height: "100%" }}>
            <Flex vertical gap={token.marginSM}>
              <Text strong>{t("page.aiTarot.questionType")}</Text>
              <Radio.Group
                value={questionType}
                onChange={(event) =>
                  setQuestionType(event.target.value as string)
                }
                style={{ width: "100%" }}
              >
                <Flex vertical gap={token.marginXS}>
                  {settings.readingTypes.map((type) => {
                    const isSelected = type.code === questionType;

                    return (
                      <Radio
                        key={type.code}
                        value={type.code}
                        style={optionStyle(isSelected)}
                      >
                        <Text strong={isSelected}>
                          {language === "vi" ? type.nameVi : type.nameEn}
                        </Text>
                      </Radio>
                    );
                  })}
                </Flex>
              </Radio.Group>
            </Flex>
          </Card>
        </Col>

        <Col xs={24} sm={12}>
          <Card size={cardSize} style={{ height: "100%" }}>
            <Flex vertical gap={token.marginSM}>
              <Text strong>{t("page.aiTarot.cardCount")}</Text>
              <Radio.Group
                value={cardCount}
                onChange={(event) => setCardCount(event.target.value as number)}
                style={{ width: "100%" }}
              >
                <Flex vertical gap={token.marginXS}>
                  {settings.readingCounts.map((item) => {
                    const isSelected = item.cardCount === cardCount;

                    return (
                      <Radio
                        key={item.cardCount}
                        value={item.cardCount}
                        style={optionStyle(isSelected)}
                      >
                        <Flex
                          justify="space-between"
                          align="center"
                          gap={token.marginSM}
                          style={{ width: "100%" }}
                        >
                          <Text strong={isSelected}>
                            {t("page.aiTarot.cardCountOption", {
                              count: item.cardCount,
                            })}
                          </Text>
                          <Text strong={isSelected}>
                            -
                            <Coin
                              type="white"
                              number={item.whiteCoins}
                              isTooltip={false}
                            />
                          </Text>
                        </Flex>
                      </Radio>
                    );
                  })}
                </Flex>
              </Radio.Group>
            </Flex>
          </Card>
        </Col>
      </Row>

      <Card size={cardSize}>
        <Flex vertical gap={token.marginSM}>
          <Text strong>{t("page.aiTarot.positions")}</Text>
          {positions.length > 0 ? (
            <Flex vertical gap={token.marginSM}>
              {positions.map((position, index) => (
                <Flex key={index} gap={token.marginSM} align="center">
                  <span
                    style={{
                      display: "inline-flex",
                      alignItems: "center",
                      justifyContent: "center",
                      flex: "none",
                      width: 24,
                      height: 24,
                      borderRadius: "50%",
                      fontSize: 12,
                      fontWeight: 600,
                      color: token.colorPrimary,
                      background: token.colorPrimaryBg,
                    }}
                  >
                    {index + 1}
                  </span>
                  <Text>{position}</Text>
                </Flex>
              ))}
            </Flex>
          ) : (
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description={t("page.aiTarot.noPositions")}
            />
          )}
        </Flex>
      </Card>

      <div
        style={{
          position: isMobile ? "sticky" : "static",
          bottom: 0,
          zIndex: 10,
          background: token.colorBgContainer,
          border: `1px solid ${token.colorBorderSecondary}`,
          borderRadius: token.borderRadiusLG,
          padding: isMobile ? token.paddingSM : token.padding,
          paddingBottom: isMobile
            ? `calc(env(safe-area-inset-bottom) + ${token.paddingSM}px)`
            : token.padding,
          boxShadow: isMobile ? token.boxShadowSecondary : undefined,
        }}
      >
        <Flex vertical gap={token.marginSM}>
          <Flex
            justify="space-between"
            align="center"
            wrap
            gap={token.marginSM}
          >
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
      </div>
    </Flex>
  );
}
