import { Card, Divider, Flex, Skeleton, theme } from "antd";

/** Skeleton placeholder matching the BalanceOverview layout. */
export function BalanceOverviewLoading() {
  const { token } = theme.useToken();

  const renderStat = (key: string) => (
    <Flex
      key={key}
      vertical
      align="center"
      gap={token.marginXS}
      style={{ flex: 1 }}
    >
      <Skeleton.Input active size="small" style={{ width: 64 }} />
      <Skeleton.Input active size="small" style={{ width: 96, height: 28 }} />
    </Flex>
  );

  return (
    <Card
      role="status"
      aria-busy
      style={{
        borderRadius: token.borderRadiusLG,
        boxShadow: token.boxShadowSecondary,
        marginBottom: 24,
      }}
      styles={{
        body: {
          display: "flex",
          alignItems: "center",
          padding: `${token.paddingLG}px ${token.marginLG}px`,
        },
      }}
    >
      {renderStat("white")}
      <Divider vertical style={{ height: 84, margin: 0 }} />
      {renderStat("red")}
    </Card>
  );
}
