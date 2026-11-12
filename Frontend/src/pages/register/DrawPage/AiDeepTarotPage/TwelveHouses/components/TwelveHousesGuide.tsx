import { AI_DEEP_TAROT_POSITIONS } from "@/constants";
import { Card, Flex, List, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";

const { Text, Paragraph } = Typography;

/** Static explainer of the 12 houses, plus the list of houses in the spread. */
export default function TwelveHousesGuide() {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const positions = AI_DEEP_TAROT_POSITIONS.twelveHouses;

  return (
    <Flex vertical gap={token.marginLG}>
      <Card>
        <Text strong>
          {t("page.aiDeepTarot.spreads.twelveHouses.what.title")}
        </Text>
        <Paragraph style={{ marginTop: token.marginSM, marginBottom: 0 }}>
          {t("page.aiDeepTarot.spreads.twelveHouses.what.body")}
        </Paragraph>
      </Card>

      <Card>
        <Text strong>{t("page.aiDeepTarot.spreads.twelveHouses.why.title")}</Text>
        <List
          size="small"
          style={{ marginTop: token.marginSM }}
          dataSource={[
            "overview",
            "blockage",
            "forecast",
          ] as const}
          renderItem={(item) => (
            <List.Item style={{ paddingBlock: token.paddingXS }}>
              <Text>
                {t(`page.aiDeepTarot.spreads.twelveHouses.why.items.${item}`)}
              </Text>
            </List.Item>
          )}
        />
      </Card>

      <Card>
        <Text strong>{t("page.aiDeepTarot.spreads.twelveHouses.when.title")}</Text>
        <List
          size="small"
          style={{ marginTop: token.marginSM }}
          dataSource={[
            "milestone",
            "lost",
            "selfReview",
          ] as const}
          renderItem={(item) => (
            <List.Item style={{ paddingBlock: token.paddingXS }}>
              <Text>
                {t(`page.aiDeepTarot.spreads.twelveHouses.when.items.${item}`)}
              </Text>
            </List.Item>
          )}
        />
      </Card>

      <Card>
        <Text strong>
          {t("page.aiDeepTarot.spreads.twelveHouses.positions.title")}
        </Text>
        <Paragraph type="secondary" style={{ marginTop: token.marginXS }}>
          {t("page.aiDeepTarot.spreads.twelveHouses.positions.hint")}
        </Paragraph>
        <Flex vertical gap={token.marginXS} style={{ marginTop: token.marginSM }}>
          {positions.map((positionKey, index) => (
            <Flex key={positionKey} gap={token.marginSM} align="baseline">
              <Text type="secondary">{index + 1}.</Text>
              <Text>
                {t(`page.aiDeepTarot.spreads.twelveHouses.position.${positionKey}`)}
              </Text>
            </Flex>
          ))}
        </Flex>
      </Card>
    </Flex>
  );
}
