import { Card, Col, Flex, Grid, Row, Skeleton, theme } from "antd";

const { useBreakpoint } = Grid;

/** Skeleton placeholder matching the pending CheckoutCard layout. */
export function CheckoutCardLoading() {
  const { token } = theme.useToken();
  const screens = useBreakpoint();
  const isMobile = !screens.md;
  const qrSize = isMobile ? 200 : 240;

  return (
    <Row gutter={[token.marginMD, token.marginMD]} role="status" aria-busy>
      <Col xs={24} md={11}>
        <Card style={{ height: "100%" }} styles={{ body: { height: "100%" } }}>
          <Flex vertical gap={token.marginMD} style={{ height: "100%" }}>
            <Flex
              justify="space-between"
              align="center"
              wrap
              gap={token.marginXS}
            >
              <Skeleton.Input
                active
                size="small"
                style={{ width: 180, height: 24 }}
              />
              <Skeleton.Input active size="small" style={{ width: 90 }} />
            </Flex>

            <Flex vertical align="center" gap={token.marginSM}>
              <Skeleton.Input active size="small" style={{ width: 120 }} />
              <Skeleton.Node
                active
                style={{
                  width: qrSize,
                  height: qrSize,
                  borderRadius: token.borderRadiusLG,
                }}
              />
            </Flex>

            <Skeleton.Input
              active
              size="small"
              style={{ width: "70%", alignSelf: "center" }}
            />

            <div style={{ flex: 1 }} />

            <Skeleton.Button active block style={{ height: 40 }} />
          </Flex>
        </Card>
      </Col>

      <Col xs={24} md={13}>
        <Card style={{ height: "100%" }} styles={{ body: { height: "100%" } }}>
          <Flex vertical gap={token.marginMD} style={{ height: "100%" }}>
            <Skeleton.Input
              active
              size="small"
              style={{ width: 160, height: 24 }}
            />

            <Flex vertical gap={token.marginXS}>
              <Skeleton.Input active size="small" style={{ width: 80 }} />
              <Skeleton.Input
                active
                size="small"
                style={{ width: 160, height: 36 }}
              />
            </Flex>

            <Flex vertical gap={token.marginSM}>
              {Array.from({ length: 3 }).map((_, index) => (
                <Flex
                  key={index}
                  justify="space-between"
                  align="center"
                  gap={token.marginSM}
                >
                  <Skeleton.Input active size="small" style={{ width: 90 }} />
                  <Skeleton.Input active size="small" style={{ width: 120 }} />
                </Flex>
              ))}
            </Flex>

            <div style={{ flex: 1 }} />

            <Flex vertical gap={token.marginSM}>
              <Skeleton.Button active block style={{ height: 40 }} />
              <Skeleton.Button active block style={{ height: 44 }} />
            </Flex>
          </Flex>
        </Card>
      </Col>
    </Row>
  );
}
