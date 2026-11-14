import { Card, Col, Flex, Grid, Row, Skeleton, theme } from "antd";

/** Skeleton placeholder matching the SelectReadingStep layout. */
export function SelectReadingStepLoading() {
  const { token } = theme.useToken();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const cardSize = isMobile ? "small" : "medium";

  const renderOptions = (count: number) => (
    <Flex vertical gap={token.marginXS}>
      {Array.from({ length: count }).map((_, index) => (
        <Skeleton.Button
          key={index}
          active
          block
          style={{ height: 44, borderRadius: token.borderRadiusLG }}
        />
      ))}
    </Flex>
  );

  return (
    <Flex
      vertical
      gap={token.marginLG}
      role="status"
      aria-busy
      aria-label="Loading AI tarot options"
    >
      <Row gutter={[token.marginMD, token.marginMD]}>
        <Col xs={24} sm={12}>
          <Card size={cardSize} style={{ height: "100%" }}>
            <Flex vertical gap={token.marginSM}>
              <Skeleton.Input active size="small" style={{ width: 120 }} />
              {renderOptions(4)}
            </Flex>
          </Card>
        </Col>

        <Col xs={24} sm={12}>
          <Card size={cardSize} style={{ height: "100%" }}>
            <Flex vertical gap={token.marginSM}>
              <Skeleton.Input active size="small" style={{ width: 100 }} />
              {renderOptions(3)}
            </Flex>
          </Card>
        </Col>
      </Row>

      <Card size={cardSize}>
        <Flex vertical gap={token.marginSM}>
          <Skeleton.Input active size="small" style={{ width: 180 }} />
          {Array.from({ length: 3 }).map((_, index) => (
            <Flex key={index} gap={token.marginSM} align="center">
              <Skeleton.Avatar active size={24} />
              <Skeleton.Input active size="small" style={{ width: "60%" }} />
            </Flex>
          ))}
        </Flex>
      </Card>

      <div
        style={{
          background: token.colorBgContainer,
          border: `1px solid ${token.colorBorderSecondary}`,
          borderRadius: token.borderRadiusLG,
          padding: isMobile ? token.paddingSM : token.padding,
          paddingBottom: isMobile
            ? `calc(env(safe-area-inset-bottom) + ${token.paddingSM}px)`
            : token.padding,
          boxShadow: isMobile ? token.boxShadowSecondary : undefined,
        }}
      >
        <Flex vertical gap={token.marginSM}>
          <Flex
            justify="space-between"
            align="center"
            wrap
            gap={token.marginSM}
          >
            <Skeleton.Input active size="small" style={{ width: 150 }} />
            <Skeleton.Input active size="small" style={{ width: 120 }} />
          </Flex>
          <Skeleton.Button active block style={{ height: 40 }} />
        </Flex>
      </div>
    </Flex>
  );
}
