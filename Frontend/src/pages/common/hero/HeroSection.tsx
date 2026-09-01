import { Card, Row, theme } from "antd";
import type { UserRole } from "@/constants";
import HeroContent from "./HeroContent";
import HeroIllustration from "./HeroIllustration";

interface HeroSectionProps {
  role?: UserRole;
}

export default function HeroSection({ role }: HeroSectionProps) {
  const { token } = theme.useToken();

  return (
    <div style={{ maxWidth: 1200, margin: "0 auto", paddingBottom: 40 }}>
      <Card
        style={{
          borderRadius: token.borderRadiusLG,
          background: `linear-gradient(135deg, ${token.colorBgElevated} 0%, ${token.colorBgContainer} 100%)`,
          border: `1px solid ${token.colorBorder}`,
          boxShadow: token.boxShadowSecondary,
          marginBottom: 32,
          overflow: "hidden",
          position: "relative",
        }}
        styles={{ body: { padding: `${token.paddingXL}px ${token.paddingLG}px` } }}
      >
        <Row align="middle" gutter={[32, 32]}>
          <HeroContent role={role} />
          <HeroIllustration />
        </Row>
      </Card>
    </div>
  );
}
