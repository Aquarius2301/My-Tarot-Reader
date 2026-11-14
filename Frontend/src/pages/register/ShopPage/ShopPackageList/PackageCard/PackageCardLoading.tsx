import { Card, Flex, Grid, Skeleton, theme } from "antd";

const { useBreakpoint } = Grid;

/** Skeleton placeholder matching the PackageCard layout. */
export function PackageCardLoading() {
  const { token } = theme.useToken();
  const screens = useBreakpoint();
  const isMobile = !screens.md;

  return (
    <Card
      style={{ height: "100%" }}
      styles={{
        body: {
          height: "100%",
          padding: isMobile ? token.padding : token.paddingLG,
        },
      }}
    >
      <Flex vertical gap={token.marginMD} style={{ height: "100%" }}>
        <Flex justify="space-between" align="flex-start" gap={token.marginSM}>
          <Flex align="baseline" gap={token.marginXS}>
            <Skeleton.Input
              active
              size="small"
              style={{ width: 72, height: 28 }}
            />
            <Skeleton.Input active size="small" style={{ width: 48 }} />
          </Flex>
          <Skeleton.Button
            active
            size="small"
            style={{ width: 72, height: 24 }}
          />
        </Flex>

        <Skeleton.Button
          active
          size="small"
          style={{ width: 110, height: 22 }}
        />

        <div style={{ flex: 1 }} />

        <Flex justify="flex-end">
          <Skeleton.Input
            active
            size="small"
            style={{ width: 120, height: 20 }}
          />
        </Flex>

        <Skeleton.Button active block style={{ height: 40 }} />
      </Flex>
    </Card>
  );
}
