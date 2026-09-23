import { Card, Empty, Table, Tag, Typography, theme, type TableProps } from "antd";
import { useTranslation } from "react-i18next";
import i18n from "@/i18n";
import type { WhiteCoinBatchItem } from "@/types";
import { convertISOToDate } from "@/utils";

const { Title, Text } = Typography;

export interface WhiteCoinBatchesCardProps {
  batches: WhiteCoinBatchItem[];
}

/**
 * Lists the user's active white coin batches in the backend-provided order
 * (expiry ascending), highlighting the deadline of each batch with a tag.
 */
export default function WhiteCoinBatchesCard({
  batches,
}: WhiteCoinBatchesCardProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const renderExpiry = (expiredAt: string) => {
    const { date, time } = convertISOToDate(expiredAt, i18n.language);
    const days = Math.ceil(
      (new Date(expiredAt).getTime() - Date.now()) / 86_400_000,
    );
    const tagColor = days <= 3 ? "error" : days <= 7 ? "warning" : "default";
    const label =
      days <= 0 ? t("page.wallet.expiresToday") : t("page.wallet.daysLeft", { days });

    return (
      <div style={{ display: "flex", flexDirection: "column", gap: 4 }}>
        <Text>
          {date} · {time}
        </Text>
        <Tag color={tagColor} style={{ width: "fit-content", marginInlineEnd: 0 }}>
          {label}
        </Tag>
      </div>
    );
  };

  const columns: TableProps<WhiteCoinBatchItem>["columns"] = [
    {
      title: t("page.wallet.colAmount"),
      dataIndex: "amount",
      key: "amount",
      align: "center",
      render: (amount: number) => <Text strong>{amount}</Text>,
    },
    {
      title: t("page.wallet.colRemaining"),
      dataIndex: "remainingAmount",
      key: "remainingAmount",
      align: "center",
      render: (remaining: number) => <Text>{remaining}</Text>,
    },
    {
      title: t("page.wallet.colExpiresAt"),
      dataIndex: "expiredAt",
      key: "expiredAt",
      render: renderExpiry,
    },
  ];

  return (
    <Card
      style={{
        borderRadius: token.borderRadiusLG,
        boxShadow: token.boxShadowSecondary,
        marginBottom: 24,
      }}
      title={
        <Title level={4} style={{ margin: 0 }}>
          {t("page.wallet.batchesTitle")}
        </Title>
      }
    >
      <Text
        type="secondary"
        style={{ display: "block", marginBottom: token.marginSM }}
      >
        {t("page.wallet.batchesSubtitle")}
      </Text>
      <Table<WhiteCoinBatchItem>
        rowKey="id"
        dataSource={batches}
        columns={columns}
        pagination={false}
        scroll={{ x: "max-content" }}
        locale={{ emptyText: <Empty description={t("page.wallet.empty")} /> }}
      />
    </Card>
  );
}