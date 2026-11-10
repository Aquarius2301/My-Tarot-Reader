import { AI_DEEP_TAROT_POSITIONS } from "@/constants";
import { getDeepTarotMonthLabel } from "@/utils";
import { Card, Flex, List, Tag, Typography, theme } from "antd";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";

const { Text, Paragraph } = Typography;

/**
 * Static explainer of the 12 months spread, plus a live preview of the exact
 * calendar months the user is about to draw for and the theme of each one.
 */
export default function TwelveMonthsGuide() {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const positions = AI_DEEP_TAROT_POSITIONS.twelveMonths;

  // The 12 months start after the current one, so resolve the labels once on
  // mount: re-computing on every render would shift the whole spread if the
  // user leaves the page open across a month boundary.
  const now = useMemo(() => new Date().toISOString(), []);
  const monthLabels = useMemo(
    () => positions.map((_, index) => getDeepTarotMonthLabel(now, index + 1)),
    [positions, now],
  );
  const readingMonth = useMemo(() => getDeepTarotMonthLabel(now, 0), [now]);

  return (
    <Flex vertical gap={token.marginLG}>
      <Card>
        <Text strong>
          {t("page.aiDeepTarot.spreads.twelveMonths.what.title")}
        </Text>
        <Paragraph style={{ marginTop: token.marginSM, marginBottom: 0 }}>
          {t("page.aiDeepTarot.spreads.twelveMonths.what.body")}
        </Paragraph>
      </Card>

      <Card>
        <Text strong>
          {t("page.aiDeepTarot.spreads.twelveMonths.why.title")}
        </Text>
        <List
          size="small"
          style={{ marginTop: token.marginSM }}
          dataSource={["forecast", "timing", "prepare"] as const}
          renderItem={(item) => (
            <List.Item style={{ paddingBlock: token.paddingXS }}>
              <Text>
                {t(`page.aiDeepTarot.spreads.twelveMonths.why.items.${item}`)}
              </Text>
            </List.Item>
          )}
        />
      </Card>

      <Card>
        <Text strong>
          {t("page.aiDeepTarot.spreads.twelveMonths.when.title")}
        </Text>
        <List
          size="small"
          style={{ marginTop: token.marginSM }}
          dataSource={["milestone", "decision", "quiet"] as const}
          renderItem={(item) => (
            <List.Item style={{ paddingBlock: token.paddingXS }}>
              <Text>
                {t(`page.aiDeepTarot.spreads.twelveMonths.when.items.${item}`)}
              </Text>
            </List.Item>
          )}
        />
      </Card>

      <Card>
        <Text strong>
          {t("page.aiDeepTarot.spreads.twelveMonths.preview.title")}
        </Text>
        <Paragraph type="secondary" style={{ marginTop: token.marginXS }}>
          {t("page.aiDeepTarot.spreads.twelveMonths.preview.hint")}
        </Paragraph>
        <Flex
          vertical
          gap={token.marginXS}
          style={{ marginTop: token.marginSM }}
        >
          <Text>
            {t("page.aiDeepTarot.spreads.twelveMonths.preview.currentMonth", {
              month: readingMonth,
            })}
          </Text>
          <Flex align="center" gap={token.marginXS} wrap>
            <Tag color="blue">{monthLabels[0]}</Tag>
            <Text type="secondary">→</Text>
            <Tag color="blue">{monthLabels[monthLabels.length - 1]}</Tag>
          </Flex>
        </Flex>
      </Card>

      {/* <Card>
        <Text strong>
          {t("page.aiDeepTarot.spreads.twelveMonths.positions.title")}
        </Text>
        <Paragraph type="secondary" style={{ marginTop: token.marginXS }}>
          {t("page.aiDeepTarot.spreads.twelveMonths.positions.hint")}
        </Paragraph>
        <Flex vertical gap={token.marginXS} style={{ marginTop: token.marginSM }}>
          {positions.map((positionKey, index) => (
            <Flex key={positionKey} gap={token.marginSM} align="baseline" wrap>
              <Text type="secondary" style={{ minWidth: 24 }}>
                {index + 1}.
              </Text>
              <Text strong style={{ minWidth: 64 }}>
                {monthLabels[index]}
              </Text>
              <Text type="secondary">
                {t(
                  `page.aiDeepTarot.spreads.twelveMonths.monthThemes.${positionKey}`,
                )}
              </Text>
            </Flex>
          ))}
        </Flex>
      </Card> */}
    </Flex>
  );
}
