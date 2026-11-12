import { CARD_ASPECT, CARD_SIZE_WIDTH } from "@/components";
import { Card, Divider, Flex, Skeleton, theme } from "antd";

export function HistoryAiDeepTarotItemLoading({
  cardCount = 6,
}: {
  cardCount?: number;
}) {
  const { token } = theme.useToken();
  const width = CARD_SIZE_WIDTH.sm;
  const height = Math.round(width * CARD_ASPECT);
  const radius = Math.max(6, width * 0.06);

  return (
    <Card
      role="status"
      aria-busy
      aria-label="Loading AI deep tarot reading"
      title={
        <Flex align="center" gap={token.marginXS}>
          <Skeleton.Avatar active size="small" shape="circle" />
          <Skeleton.Input active size="small" style={{ width: 220 }} />
        </Flex>
      }
      extra={<Skeleton.Button active size="small" shape="circle" />}
    >
      <Flex vertical gap={16}>
        <Flex gap={8} style={{ overflowX: "hidden" }}>
          {Array.from({ length: cardCount }).map((_, index) => (
            <div key={index} style={{ flex: "0 0 auto" }}>
              <Skeleton.Node
                active
                style={{ width, height, borderRadius: radius }}
              />
            </div>
          ))}
        </Flex>

        <Divider size="small" />

        <Flex wrap align="center" gap={16}>
          <Skeleton.Button active size="small" style={{ width: 80 }} />
          <Flex align="center" gap={8}>
            <Skeleton.Input active size="small" style={{ width: 90 }} />
            <Skeleton.Input active size="small" style={{ width: 56 }} />
          </Flex>
        </Flex>

        <Skeleton.Input active size="small" style={{ width: "60%" }} />
        <Skeleton active title={false} paragraph={{ rows: 3 }} />
      </Flex>
    </Card>
  );
}
