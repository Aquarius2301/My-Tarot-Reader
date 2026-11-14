import { CARD_ASPECT, CARD_SIZE_WIDTH } from "@/components";
import { Card, Flex, Skeleton, theme } from "antd";

export function AiDeepTarotResultItemLoading() {
  const { token } = theme.useToken();
  const width = CARD_SIZE_WIDTH.sm;
  const height = Math.round(width * CARD_ASPECT);
  const radius = Math.max(6, width * 0.06);

  const cardCount = 6;

  return (
    <Flex vertical gap={token.marginLG} role="status" aria-busy>
      {/* Crossroads info card (optional) */}
      <Card extra={<Skeleton.Input active style={{ width: 60 }} />}>
        <Skeleton active title={false} paragraph={{ rows: 2 }} />
      </Card>

      {/* Overview */}
      <Card>
        <Skeleton.Input
          active
          size="small"
          style={{ width: 140, marginBottom: token.marginSM }}
        />
        <Skeleton active title={false} paragraph={{ rows: 3 }} />
      </Card>

      {/* Position cards */}
      {Array.from({ length: cardCount }).map((_, index) => (
        <Card key={index}>
          <Flex align="center" gap={token.marginSM} wrap>
            <Skeleton.Node
              active
              style={{ width, height, borderRadius: radius }}
            />
            <Flex vertical style={{ minWidth: 0 }}>
              <Skeleton.Input
                active
                size="small"
                style={{ width: 120, marginBottom: token.marginXS }}
              />
              <Skeleton.Input
                active
                size="small"
                style={{ width: 180, marginBottom: token.marginXS }}
              />
              <Skeleton.Input active size="small" style={{ width: 200 }} />
            </Flex>
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
