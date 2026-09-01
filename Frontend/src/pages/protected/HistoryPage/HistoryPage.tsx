import { Card, Spin, Typography, Empty, Tag, Flex, theme } from "antd";
import { ClockCircleOutlined, CalendarOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { TarotCard } from "@/components";
import { useGetHistoryReadings } from "@/hooks/api/history.hooks";
import { convertISOToDate } from "@/utils";
import type { HistoryItem } from "@/types";

const { Title, Text } = Typography;

export default function HistoryPage() {
  const { t, i18n } = useTranslation();
  const { data, isLoading } = useGetHistoryReadings();
  const { token } = theme.useToken();

  if (isLoading) {
    return <Spin fullscreen size="large" />;
  }

  return (
    <div
      style={{
        maxWidth: 1080,
        margin: "0 auto",
        padding: `${token.paddingLG}px ${token.paddingMD}px`,
      }}
    >
      {/* Header Section */}
      <div style={{ textAlign: "center", marginBottom: token.marginXL }}>
        <Title
          level={2}
          style={{
            margin: 0,
          }}
        >
          {t("page.history.title")}
        </Title>
        <Text type="secondary">{t("page.history.subtitle")}</Text>
      </div>

      {/* Empty State */}
      {(!data || data.histories.length === 0) && (
        <Empty
          description={t("page.history.empty")}
          style={{ margin: `${token.marginXXL}px 0` }}
        />
      )}

      {/* Card Grid */}
      <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(auto-fill, minmax(220px, 1fr))",
          gap: token.marginLG,
        }}
      >
        {data?.histories.map((item: HistoryItem, index: number) => {
          const { date, time } = convertISOToDate(
            item.createdAt,
            i18n.language,
          );

          return (
            <Card
              key={item.id || item.cardCode + item.createdAt + index}
              hoverable
              style={{
                borderRadius: token.borderRadiusLG,
                overflow: "hidden",
                border: `1px solid ${token.colorBorderSecondary}`,
                backdropFilter: "blur(10px)",
                boxShadow: token.boxShadowTertiary,
                transition: "all 0.3s cubic-bezier(0.4, 0, 0.2, 1)",
              }}
              styles={{
                body: {
                  display: "flex",
                  flexDirection: "column",
                  alignItems: "center",
                  padding: `${token.paddingLG}px ${token.paddingMD}px ${token.paddingMD}px`,
                },
              }}
            >
              {/* Tarot Card Preview Container */}
              <div
                style={{
                  marginBottom: token.marginMD,
                  transition: "transform 0.3s ease",
                  filter: `drop-shadow(0 4px 12px ${token.colorPrimary}26)`,
                }}
              >
                <TarotCard
                  cardCode={item.cardCode}
                  isUpright={!item.isReversed}
                  size="md"
                />
              </div>

              {/* Status Badge */}
              <Tag
                color={item.isReversed ? "volcano" : "purple"}
                style={{
                  borderRadius: token.borderRadiusSM,
                  padding: "2px 10px",
                  marginBottom: token.marginSM,
                  border: "none",
                  fontWeight: 500,
                }}
              >
                {item.isReversed
                  ? t("page.history.reversed")
                  : t("page.history.upright")}
              </Tag>

              {/* Date & Time Footer */}
              <Flex
                gap="small"
                vertical
                style={{
                  width: "100%",
                  borderTop: `1px solid ${token.colorBorderSecondary}`,
                  paddingTop: token.paddingSM,
                }}
              >
                <Flex align="center" justify="center" gap={6}>
                  <CalendarOutlined style={{ fontSize: 12 }} />
                  <Text>{date}</Text>
                  <ClockCircleOutlined
                    style={{ fontSize: 12, marginLeft: 6 }}
                  />
                  <Text>{time}</Text>
                </Flex>
              </Flex>
            </Card>
          );
        })}
      </div>
    </div>
  );
}
