import { useState } from "react";
import { App, Button, Spin, Typography, theme } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { ErrorComponent } from "@/components";
import { useDeleteShopPackage, useGetAdminShopPackages } from "@/hooks/api";
import type { GetAllShopPackagesItem } from "@/types";
import { getErrorMessage } from "@/utils";
import {
  DeleteShopPackageModal,
  ShopPackageFormModal,
  ShopPackageTable,
} from "./components";

const { Title, Text } = Typography;

/**
 * Admin shop package management: lists every package (including disabled ones)
 * and allows creating, editing and soft-deleting them.
 */
export default function AdminShopPackagesPage() {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const { message } = App.useApp();

  const { data, isLoading, refetch } = useGetAdminShopPackages();
  const deleteMutation = useDeleteShopPackage();

  const [formOpen, setFormOpen] = useState(false);
  const [editingPackage, setEditingPackage] =
    useState<GetAllShopPackagesItem | null>(null);
  const [deletingPackage, setDeletingPackage] =
    useState<GetAllShopPackagesItem | null>(null);

  const openCreateForm = () => {
    setEditingPackage(null);
    setFormOpen(true);
  };

  const openEditForm = (item: GetAllShopPackagesItem) => {
    setEditingPackage(item);
    setFormOpen(true);
  };

  const closeForm = () => {
    setFormOpen(false);
    setEditingPackage(null);
  };

  const handleConfirmDelete = () => {
    if (!deletingPackage) {
      return;
    }

    deleteMutation.mutate(deletingPackage.id, {
      onSuccess: () => {
        message.success(t("page.admin.shopPackages.deleteSuccess"));
        setDeletingPackage(null);
      },
      onError: (error) => message.error(getErrorMessage(error)),
    });
  };

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (data === undefined) {
    return <ErrorComponent type="server" onRetry={refetch} />;
  }

  return (
    <div style={{ maxWidth: 1080, margin: "0 auto" }}>
      <div
        style={{
          display: "flex",
          flexWrap: "wrap",
          alignItems: "flex-start",
          justifyContent: "space-between",
          gap: token.marginMD,
          marginBottom: token.marginLG,
        }}
      >
        <div>
          <Title level={2} style={{ marginBottom: 4 }}>
            {t("page.admin.shopPackages.title")}
          </Title>
          <Text type="secondary">
            {t("page.admin.shopPackages.subtitle")}
          </Text>
        </div>

        <Button type="primary" icon={<PlusOutlined />} onClick={openCreateForm}>
          {t("page.admin.shopPackages.addButton")}
        </Button>
      </div>

      <ShopPackageTable
        items={data.items}
        onEdit={openEditForm}
        onDelete={(item) => setDeletingPackage(item)}
      />

      <ShopPackageFormModal
        open={formOpen}
        packageItem={editingPackage}
        onClose={closeForm}
      />

      <DeleteShopPackageModal
        open={deletingPackage !== null}
        code={deletingPackage?.code}
        loading={deleteMutation.isPending}
        onClose={() => setDeletingPackage(null)}
        onConfirm={handleConfirmDelete}
      />
    </div>
  );
}
