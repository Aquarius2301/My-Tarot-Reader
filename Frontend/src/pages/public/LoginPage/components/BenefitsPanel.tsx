import { Avatar, Col, List, Space, Typography, theme } from "antd";
import type { BenefitItem } from "../LoginPage";

const { Title, Paragraph, Text } = Typography;

interface BenefitsPanelProps {
  benefits: BenefitItem[];
  heading: string;
  subtitle: string;
}

export default function BenefitsPanel({
  benefits,
  heading,
  subtitle,
}: BenefitsPanelProps) {
  const { token } = theme.useToken();

  return (
    <Col
      xs={24}
      md={12}
      style={{
        padding: `${token.paddingXL}px ${token.paddingLG}px`,
        background: `linear-gradient(135deg, ${token.colorPrimaryBg} 0%, ${token.colorBgElevated} 100%)`,
        display: "flex",
        flexDirection: "column",
        justifyContent: "center",
      }}
    >
      <Space vertical size="small" style={{ marginBottom: 24 }}>
        <Title level={3} style={{ margin: 0 }}>
          {heading}
        </Title>
        <Paragraph type="secondary" style={{ margin: 0 }}>
          {subtitle}
        </Paragraph>
      </Space>

      <List<BenefitItem>
        itemLayout="horizontal"
        dataSource={benefits}
        renderItem={(item) => (
          <List.Item style={{ padding: "12px 0", border: "none" }}>
            <List.Item.Meta
              avatar={
                <Avatar
                  style={{
                    backgroundColor: token.colorBgContainer,
                    boxShadow: token.boxShadowTertiary,
                  }}
                  icon={item.icon}
                />
              }
              title={
                <Text strong style={{ fontSize: 15 }}>
                  {item.title}
                </Text>
              }
              description={
                <Text type="secondary" style={{ fontSize: 13 }}>
                  {item.description}
                </Text>
              }
            />
          </List.Item>
        )}
      />
    </Col>
  );
}
