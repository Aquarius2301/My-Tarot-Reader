import { Card, Col, Row, Statistic, theme } from "antd";
import { useTranslation } from "react-i18next";

export interface StatCardsProps {
  totalGuest: number;
  totalRegistered: number;
  totalAi: number;
}

export default function StatCards({
  totalGuest,
  totalRegistered,
  totalAi,
}: StatCardsProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const cardStyle = {
    borderRadius: token.borderRadiusLG,
    boxShadow: token.boxShadowSecondary,
  };

  return (
    <Row gutter={[16, 16]}>
      <Col xs={24} md={8}>
        <Card style={cardStyle}>
          <Statistic
            title={t("page.admin.statistics.guest")}
            value={totalGuest}
            precision={0}
          />
        </Card>
      </Col>
      <Col xs={24} md={8}>
        <Card style={cardStyle}>
          <Statistic
            title={t("page.admin.statistics.registered")}
            value={totalRegistered}
            precision={0}
          />
        </Card>
      </Col>
      <Col xs={24} md={8}>
        <Card style={cardStyle}>
          <Statistic
            title={t("page.admin.statistics.ai")}
            value={totalAi}
            precision={0}
          />
        </Card>
      </Col>
    </Row>
  );
}
