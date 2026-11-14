import { Card, Flex, Skeleton, theme } from "antd";

/** Skeleton placeholder matching the WhiteCoinBatchesCard layout. */
export function WhiteCoinBatchesCardLoading() {
  const { token } = theme.useToken();

  return (
    <Card
      role="status"
      aria-busy
      style={{
        borderRadius: token.borderRadiusLG,
        boxShadow: token.boxShadowSecondary,
        marginBottom: 24,
      }}
      title={<Skeleton.Input active size="small" style={{ width: 160 }} />}
    >
      <Skeleton.Input
        active
        size="small"
        style={{ width: "55%", marginBottom: token.marginSM }}
      />

      <Flex vertical gap={token.marginMD}>
        {Array.from({ length: 3 }).map((_, index) => (
          <Flex
            key={index}
            justify="space-between"
            align="center"
            gap={token.marginMD}
          >
            <Skeleton.Input active size="small" style={{ width: 80 }} />
            <Skeleton.Input active size="small" style={{ width: 80 }} />
            <Skeleton.Input active size="small" style={{ width: 140 }} />
          </Flex>
        ))}
      </Flex>
    </Card>
  );
}
