import { Button, Empty, Space, Table, Tag, Typography } from "antd";
import type { TableColumnsType } from "antd";
import { DeleteOutlined, EditOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import type { AdminAiTarotReadingTypeItem } from "@/types";

const { Text } = Typography;

export interface ReadingTypeTableProps {
  items: AdminAiTarotReadingTypeItem[];
  onEdit: (item: AdminAiTarotReadingTypeItem) => void;
  onDelete: (item: AdminAiTarotReadingTypeItem) => void;
}

export default function ReadingTypeTable({
  items,
  onEdit,
  onDelete,
}: ReadingTypeTableProps) {
  const { t } = useTranslation();

  const columns: TableColumnsType<AdminAiTarotReadingTypeItem> = [
    {
      title: t("page.admin.aiTarotSettings.readingTypes.table.code"),
      dataIndex: "code",
      key: "code",
      width: 160,
      render: (code: string) => <Text code>{code}</Text>,
    },
    {
      title: t("page.admin.aiTarotSettings.readingTypes.table.nameEn"),
      dataIndex: "nameEn",
      key: "nameEn",
      ellipsis: true,
    },
    {
      title: t("page.admin.aiTarotSettings.readingTypes.table.nameVi"),
      dataIndex: "nameVi",
      key: "nameVi",
      ellipsis: true,
    },
    {
      title: t("page.admin.aiTarotSettings.readingTypes.table.status"),
      dataIndex: "isEnabled",
      key: "isEnabled",
      width: 120,
      render: (isEnabled: boolean) => (
        <Tag color={isEnabled ? "success" : "default"}>
          {isEnabled
            ? t("page.admin.aiTarotSettings.readingTypes.table.enabled")
            : t("page.admin.aiTarotSettings.readingTypes.table.disabled")}
        </Tag>
      ),
    },
    {
      title: t("page.admin.aiTarotSettings.readingTypes.table.actions"),
      key: "actions",
      width: 120,
      render: (_, item) => (
        <Space size={4}>
          <Button
            type="text"
            icon={<EditOutlined />}
            aria-label={t("page.admin.aiTarotSettings.readingTypes.editAction")}
            title={t("page.admin.aiTarotSettings.readingTypes.editAction")}
            onClick={() => onEdit(item)}
          />
          <Button
            type="text"
            danger
            icon={<DeleteOutlined />}
            aria-label={t("page.admin.aiTarotSettings.readingTypes.deleteAction")}
            title={t("page.admin.aiTarotSettings.readingTypes.deleteAction")}
            onClick={() => onDelete(item)}
          />
        </Space>
      ),
    },
  ];

  return (
    <Table<AdminAiTarotReadingTypeItem>
      rowKey="id"
      columns={columns}
      dataSource={items}
      pagination={false}
      scroll={{ x: 700 }}
      locale={{
        emptyText: (
          <Empty
            description={t("page.admin.aiTarotSettings.readingTypes.empty")}
          />
        ),
      }}
    />
  );
}
