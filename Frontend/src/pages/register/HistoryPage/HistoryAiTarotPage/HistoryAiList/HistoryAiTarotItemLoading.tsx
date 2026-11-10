import { CARD_ASPECT, CARD_SIZE_WIDTH } from "@/components";
import { Card, Divider, Flex, Skeleton } from "antd";

export function HistoryAiTarotItemLoading({
  cardCount = 3,
}: {
  cardCount?: number;
}) {
  const width = CARD_SIZE_WIDTH.sm;
  const height = Math.round(width * CARD_ASPECT);
  const radius = Math.max(6, width * 0.06);

  return (
    <Card
      role="status"
      aria-busy
      aria-label="Loading AI tarot reading"
      title={<Skeleton.Input active size="small" style={{ width: 220 }} />}
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
          <Flex align="center" gap={6}>
            <Skeleton.Input active size="small" style={{ width: 90 }} />
            <Skeleton.Input active size="small" style={{ width: 56 }} />
          </Flex>
        </Flex>

        <Skeleton active title={false} paragraph={{ rows: 3 }} />
      </Flex>
    </Card>
  );
}
