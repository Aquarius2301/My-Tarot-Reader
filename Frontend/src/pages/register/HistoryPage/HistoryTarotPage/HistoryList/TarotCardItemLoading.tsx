import { CARD_ASPECT, CARD_SIZE_WIDTH } from "@/components";
import { Card, Divider, Flex, Skeleton } from "antd";

export function TarotCardItemLoading() {
  const width = CARD_SIZE_WIDTH.md;
  const height = Math.round(width * CARD_ASPECT);
  const radius = Math.max(6, width * 0.06);

  return (
    <Card
      role="status"
      aria-busy
      aria-label="Loading tarot card"
      title={<Skeleton.Input active size="small" style={{ width: 140 }} />}
      extra={<Skeleton.Button active size="small" shape="circle" />}
    >
      <Flex justify="center">
        <Skeleton.Node active style={{ width, height, borderRadius: radius }} />
      </Flex>

      <Divider />

      <Flex gap="small" vertical style={{ width: "100%" }}>
        <Flex wrap align="center" justify="space-between" gap={6}>
          <Skeleton.Input active size="small" style={{ width: 80 }} />
          <Skeleton.Input active size="small" style={{ width: 64 }} />
        </Flex>
      </Flex>
    </Card>
  );
}
