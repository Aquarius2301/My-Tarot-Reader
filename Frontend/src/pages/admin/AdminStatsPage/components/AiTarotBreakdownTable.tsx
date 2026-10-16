import { Card, Empty, Table, Tag, Typography, theme } from "antd";
import type { TableColumnsType } from "antd";
import { useTranslation } from "react-i18next";
import type { GetTarotStatisticAiTarotItem } from "@/types";

const { Text } = Typography;

export interface AiTarotBreakdownTableProps {
  items: GetTarotStatisticAiTarotItem[];
}

export default function AiTarotBreakdownTable({
  items,
}: AiTarotBreakdownTableProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const columns: TableColumnsType<GetTarotStatisticAiTarotItem> = [
    {
      title: t("page.admin.statistics.questionType"),
      dataIndex: "questionType",
      key: "questionType",
      render: (value: string) => (
        <Tag
          style={{
            color: token.colorPrimary,
            borderColor: token.colorPrimary,
            backgroundColor: "transparent",
          }}
        >
          {t(`page.aiTarot.questionTypes.${value}`, {
            defaultValue: value,
          })}
        </Tag>
      ),
    },
    {
      title: t("page.admin.statistics.cardCount"),
      dataIndex: "cardCount",
      key: "cardCount",
      render: (value: number) => t("page.aiTarot.cardCountOption", { count: value }),
      width: 120,
    },
    {
      title: t("page.admin.statistics.count"),
      dataIndex: "count",
      key: "count",
      render: (value: number) => <Text strong>{value}</Text>,
      width: 120,
    },
  ];

  return (
    <Card
      style={{
        borderRadius: token.borderRadiusLG,
        boxShadow: token.boxShadowSecondary,
      }}
      title={t("page.admin.statistics.breakdownTitle")}
    >
      {items.length === 0 ? (
        <Empty description={t("page.admin.statistics.empty")} />
      ) : (
        <Table<GetTarotStatisticAiTarotItem>
          rowKey={(record) => `${record.questionType}-${record.cardCount}`}
          columns={columns}
          dataSource={items}
          pagination={false}
          size="middle"
        />
      )}
    </Card>
  );
}
