import { Card, Flex, List, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";

const { Text, Paragraph } = Typography;

/** Static explainer of the crossroads spread: what it is, what it is for, when to read it. */
export default function CrossroadsGuide() {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  return (
    <Flex vertical gap={token.marginLG}>
      <Card>
        <Text strong>
          {t("page.aiDeepTarot.spreads.crossroads.what.title")}
        </Text>
        <Paragraph style={{ marginTop: token.marginSM, marginBottom: 0 }}>
          {t("page.aiDeepTarot.spreads.crossroads.what.body")}
        </Paragraph>
      </Card>

      <Card>
        <Text strong>
          {t("page.aiDeepTarot.spreads.crossroads.why.title")}
        </Text>
        <List
          size="small"
          style={{ marginTop: token.marginSM }}
          dataSource={["compare", "clarify", "direction"] as const}
          renderItem={(item) => (
            <List.Item style={{ paddingBlock: token.paddingXS }}>
              <Text>{t(`page.aiDeepTarot.spreads.crossroads.why.items.${item}`)}</Text>
            </List.Item>
          )}
        />
      </Card>

      <Card>
        <Text strong>
          {t("page.aiDeepTarot.spreads.crossroads.when.title")}
        </Text>
        <List
          size="small"
          style={{ marginTop: token.marginSM }}
          dataSource={["career", "life", "timing"] as const}
          renderItem={(item) => (
            <List.Item style={{ paddingBlock: token.paddingXS }}>
              <Text>{t(`page.aiDeepTarot.spreads.crossroads.when.items.${item}`)}</Text>
            </List.Item>
          )}
        />
      </Card>

      <Card>
        <Text strong>
          {t("page.aiDeepTarot.spreads.crossroads.guide.title")}
        </Text>
        <Paragraph type="secondary" style={{ marginTop: token.marginXS, marginBottom: 0 }}>
          {t("page.aiDeepTarot.spreads.crossroads.guide.body")}
        </Paragraph>
      </Card>
    </Flex>
  );
}
