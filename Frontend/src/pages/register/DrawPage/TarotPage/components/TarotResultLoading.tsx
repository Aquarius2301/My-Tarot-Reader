import { CARD_ASPECT, CARD_SIZE_WIDTH } from "@/components";
import { Card, Flex, Skeleton, theme } from "antd";

export function TarotResultLoading() {
  const { token } = theme.useToken();
  const width = CARD_SIZE_WIDTH.md;
  const height = Math.round(width * CARD_ASPECT);
  const radius = Math.max(6, width * 0.06);

  return (
    <Flex vertical align="center" role="status" aria-busy>
      <Skeleton.Input active style={{ width: 180, marginBottom: 12 }} />

      <Skeleton.Node
        active
        style={{
          width,
          height,
          borderRadius: radius,
          margin: `${token.marginMD}px 0`,
        }}
      />

      <Skeleton.Input
        active
        style={{ width: 220, marginBottom: token.marginMD }}
      />

      <Card
        style={{ width: "100%", textAlign: "left" }}
        extra={<Skeleton.Button active size="small" shape="round" />}
      >
        <Skeleton active title={false} paragraph={{ rows: 10 }} />
      </Card>
    </Flex>
  );
}
