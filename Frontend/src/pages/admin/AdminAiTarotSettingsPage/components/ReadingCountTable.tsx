import { Button, Col, Empty, Row, Space, Table, Tag, Typography } from "antd";
import type { TableColumnsType } from "antd";
import { DeleteOutlined, EditOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { CoinIcon } from "@/components";
import type { AdminAiTarotReadingCountItem } from "@/types";

const { Text } = Typography;

export interface ReadingCountTableProps {
  items: AdminAiTarotReadingCountItem[];
  onEdit: (item: AdminAiTarotReadingCountItem) => void;
  onDelete: (item: AdminAiTarotReadingCountItem) => void;
}

export default function ReadingCountTable({
  items,
  onEdit,
  onDelete,
}: ReadingCountTableProps) {
  const { t } = useTranslation();

  const columns: TableColumnsType<AdminAiTarotReadingCountItem> = [
    {
      title: t("page.admin.aiTarotSettings.readingCounts.table.cardCount"),
      dataIndex: "cardCount",
      key: "cardCount",
      width: 100,
      render: (cardCount: number) => <Text strong>{cardCount}</Text>,
    },
    {
      title: t("page.admin.aiTarotSettings.readingCounts.table.whiteCoins"),
      dataIndex: "whiteCoins",
      key: "whiteCoins",
      align: "right",
      width: 120,
      render: (whiteCoins: number) => (
        <Space size={4}>
          <CoinIcon variant="white" size={16} />
          <Text>{whiteCoins.toLocaleString("vi-VN")}</Text>
        </Space>
      ),
    },
    {
      title: t("page.admin.aiTarotSettings.readingCounts.table.wordRange"),
      key: "wordRange",
      width: 140,
      render: (_, item) => `${item.minWords} – ${item.maxWords}`,
    },
    {
      title: t("page.admin.aiTarotSettings.readingCounts.table.status"),
      dataIndex: "isEnabled",
      key: "isEnabled",
      width: 120,
      render: (isEnabled: boolean) => (
        <Tag color={isEnabled ? "success" : "default"}>
          {isEnabled
            ? t("page.admin.aiTarotSettings.readingCounts.table.enabled")
            : t("page.admin.aiTarotSettings.readingCounts.table.disabled")}
        </Tag>
      ),
    },
    {
      title: t("page.admin.aiTarotSettings.readingCounts.table.actions"),
      key: "actions",
      width: 120,
      render: (_, item) => (
        <Space size={4}>
          <Button
            type="text"
            icon={<EditOutlined />}
            aria-label={t("page.admin.aiTarotSettings.readingCounts.editAction")}
            title={t("page.admin.aiTarotSettings.readingCounts.editAction")}
            onClick={() => onEdit(item)}
          />
          <Button
            type="text"
            danger
            icon={<DeleteOutlined />}
            aria-label={t("page.admin.aiTarotSettings.readingCounts.deleteAction")}
            title={t("page.admin.aiTarotSettings.readingCounts.deleteAction")}
            onClick={() => onDelete(item)}
          />
        </Space>
      ),
    },
  ];

  return (
    <Table<AdminAiTarotReadingCountItem>
      rowKey="id"
      columns={columns}
      dataSource={items}
      pagination={false}
      scroll={{ x: 820 }}
      expandable={{
        expandedRowRender: (item) => (
          <Row gutter={24}>
            <Col xs={24} md={12}>
              <Text strong>
                {t("page.admin.aiTarotSettings.readingCounts.table.positionsEn")}
              </Text>
              <ol style={{ margin: "8px 0 0", paddingLeft: 20 }}>
                {item.positionsEn.map((position, index) => (
                  <li key={`${position}-${index}`}>{position}</li>
                ))}
              </ol>
            </Col>
            <Col xs={24} md={12}>
              <Text strong>
                {t("page.admin.aiTarotSettings.readingCounts.table.positionsVi")}
              </Text>
              <ol style={{ margin: "8px 0 0", paddingLeft: 20 }}>
                {item.positionsVi.map((position, index) => (
                  <li key={`${position}-${index}`}>{position}</li>
                ))}
              </ol>
            </Col>
          </Row>
        ),
      }}
      locale={{
        emptyText: (
          <Empty
            description={t("page.admin.aiTarotSettings.readingCounts.empty")}
          />
        ),
      }}
    />
  );
}
