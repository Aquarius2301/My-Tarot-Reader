import { Card, Flex, Skeleton, theme } from "antd";

/** Skeleton placeholder matching the ConvertCard layout. */
export function ConvertCardLoading() {
  const { token } = theme.useToken();

  return (
    <Card
      role="status"
      aria-busy
      style={{
        borderRadius: token.borderRadiusLG,
        boxShadow: token.boxShadowSecondary,
      }}
    >
      <Skeleton.Input
        active
        size="small"
        style={{ width: 180, height: 24, marginBottom: 4 }}
      />
      <div>
        <Skeleton.Input active size="small" style={{ width: "50%" }} />
      </div>

      <Flex
        wrap
        align="center"
        gap={token.marginMD}
        style={{ marginTop: token.marginMD }}
      >
        <div style={{ flex: 1, minWidth: 220 }}>
          <Skeleton.Input
            active
            size="small"
            style={{ width: 120, marginBottom: 4 }}
          />
          <Flex gap={token.marginXS}>
            <Skeleton.Input active style={{ flex: 1, height: 32 }} />
            <Skeleton.Button active style={{ width: 90, height: 32 }} />
          </Flex>
        </div>

        <Flex
          vertical
          align="center"
          gap={token.marginXS}
          style={{ minWidth: 180 }}
        >
          <Skeleton.Input active size="small" style={{ width: 90 }} />
          <Skeleton.Input active size="small" style={{ width: 70, height: 24 }} />
        </Flex>
      </Flex>
    </Card>
  );
}
