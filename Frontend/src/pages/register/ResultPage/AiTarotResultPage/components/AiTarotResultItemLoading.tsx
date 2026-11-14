import { CARD_ASPECT, CARD_SIZE_WIDTH } from "@/components";
import { Card, Flex, Skeleton, Space, theme } from "antd";

export function AiTarotResultItemLoading() {
  const { token } = theme.useToken();
  const width = CARD_SIZE_WIDTH.sm;
  const height = Math.round(width * CARD_ASPECT);
  const radius = Math.max(6, width * 0.06);

  const cardCount = 3;

  return (
    <Flex vertical gap={token.marginLG} role="status" aria-busy>
      {/* Overview */}
      <Card extra={<Skeleton.Button active size="small" shape="round" />}>
        <Skeleton.Input
          active
          size="small"
          style={{ width: 140, marginBottom: token.marginSM }}
        />
        <Skeleton active title={false} paragraph={{ rows: 3 }} />
      </Card>

      {/* Card meanings */}
      {Array.from({ length: cardCount }).map((_, index) => (
        <Card key={index}>
          <Flex align="center" gap={token.marginSM} wrap>
            <Skeleton.Node
              active
              style={{ width, height, borderRadius: radius }}
            />
            <Space vertical>
              <Skeleton.Input active size="small" style={{ width: 120 }} />
              <Skeleton.Input active size="small" style={{ width: 160 }} />
            </Space>
          </Flex>
          <Skeleton
            active
            title={false}
            paragraph={{ rows: 2 }}
            style={{ marginTop: token.marginSM }}
          />
        </Card>
      ))}

      {/* Overall advice */}
      <Card>
        <Skeleton.Input
          active
          size="small"
          style={{ width: 160, marginBottom: token.marginSM }}
        />
        <Skeleton active title={false} paragraph={{ rows: 2 }} />
      </Card>
    </Flex>
  );
}
