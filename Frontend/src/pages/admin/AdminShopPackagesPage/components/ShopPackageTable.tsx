import { Button, Empty, Space, Table, Tag, Typography } from "antd";
import type { TableColumnsType } from "antd";
import { DeleteOutlined, EditOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { CoinIcon } from "@/components";
import type { GetAllShopPackagesItem } from "@/types";
import { formatVnd } from "@/utils";

const { Text } = Typography;

export interface ShopPackageTableProps {
  items: GetAllShopPackagesItem[];
  onEdit: (item: GetAllShopPackagesItem) => void;
  onDelete: (item: GetAllShopPackagesItem) => void;
}

export default function ShopPackageTable({
  items,
  onEdit,
  onDelete,
}: ShopPackageTableProps) {
  const { t } = useTranslation();

  const columns: TableColumnsType<GetAllShopPackagesItem> = [
    {
      title: t("page.admin.shopPackages.table.code"),
      dataIndex: "code",
      key: "code",
      render: (code: string) => <Text code>{code}</Text>,
    },
    {
      title: t("page.admin.shopPackages.table.redCoins"),
      dataIndex: "redCoins",
      key: "redCoins",
      align: "right",
      width: 120,
      render: (value: number) => (
        <Space size={4}>
          <CoinIcon variant="red" size={16} />
          <Text strong>{value.toLocaleString("vi-VN")}</Text>
        </Space>
      ),
    },
    {
      title: t("page.admin.shopPackages.table.whiteCoins"),
      dataIndex: "whiteCoins",
      key: "whiteCoins",
      align: "right",
      width: 120,
      render: (value: number) => (
        <Space size={4}>
          <CoinIcon variant="white" size={16} />
          <Text>{value.toLocaleString("vi-VN")}</Text>
        </Space>
      ),
    },
    {
      title: t("page.admin.shopPackages.table.oldPrice"),
      dataIndex: "oldPrice",
      key: "oldPrice",
      align: "right",
      width: 140,
      render: (value: number) => (
        <Text>{value > 0 ? formatVnd(value) : "—"}</Text>
      ),
    },
    {
      title: t("page.admin.shopPackages.table.price"),
      dataIndex: "price",
      key: "price",
      align: "right",
      width: 140,
      render: (value: number) => <Text strong>{formatVnd(value)}</Text>,
    },
    {
      title: t("page.admin.shopPackages.table.status"),
      dataIndex: "isEnabled",
      key: "isEnabled",
      width: 120,
      render: (isEnabled: boolean) => (
        <Tag color={isEnabled ? "success" : "default"}>
          {isEnabled
            ? t("page.admin.shopPackages.table.enabled")
            : t("page.admin.shopPackages.table.disabled")}
        </Tag>
      ),
    },
    {
      title: t("page.admin.shopPackages.table.actions"),
      key: "actions",
      width: 120,
      render: (_, item) => (
        <Space size={4}>
          <Button
            type="text"
            icon={<EditOutlined />}
            aria-label={t("page.admin.shopPackages.editAction")}
            title={t("page.admin.shopPackages.editAction")}
            onClick={() => onEdit(item)}
          />
          <Button
            type="text"
            danger
            icon={<DeleteOutlined />}
            aria-label={t("page.admin.shopPackages.deleteAction")}
            title={t("page.admin.shopPackages.deleteAction")}
            onClick={() => onDelete(item)}
          />
        </Space>
      ),
    },
  ];

  return (
    <Table<GetAllShopPackagesItem>
      rowKey="id"
      columns={columns}
      dataSource={items}
      pagination={false}
      scroll={{ x: 900 }}
      locale={{
        emptyText: <Empty description={t("page.admin.shopPackages.empty")} />,
      }}
    />
  );
}
