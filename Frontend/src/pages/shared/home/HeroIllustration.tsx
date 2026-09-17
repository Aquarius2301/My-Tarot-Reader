import { Col, Avatar, theme } from "antd";
import { StarOutlined } from "@ant-design/icons";

export default function HeroIllustration() {
  const { token } = theme.useToken();

  return (
    <Col xs={24} md={10} style={{ textAlign: "center" }}>
      <div
        style={{
          display: "inline-block",
          padding: 24,
          borderRadius: "50%",
          background: `radial-gradient(circle, ${token.colorPrimary}33 0%, transparent 70%)`,
        }}
      >
        <Avatar
          size={160}
          icon={
            <StarOutlined
              style={{ fontSize: 72, color: token.colorPrimary }}
            />
          }
          style={{
            backgroundColor: token.colorBgElevated,
            border: `2px solid ${token.colorPrimaryHover}`,
            boxShadow: `0 0 30px ${token.colorPrimary}40`,
          }}
        />
      </div>
    </Col>
  );
}
