import { useState } from "react";
import { App, Button, Flex, Typography, theme } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useDeleteAiTarotReadingType } from "@/hooks/api";
import type { AdminAiTarotReadingTypeItem } from "@/types";
import { getErrorMessage } from "@/utils";
import {
  DeleteAiTarotSettingModal,
  ReadingTypeFormModal,
  ReadingTypeTable,
} from "./components";

const { Text } = Typography;

export interface ReadingTypesTabProps {
  items: AdminAiTarotReadingTypeItem[];
}

/** Question topic tab: lists, creates, edits and deletes reading types. */
export default function ReadingTypesTab({ items }: ReadingTypesTabProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const { message } = App.useApp();
  const deleteMutation = useDeleteAiTarotReadingType();

  const [formOpen, setFormOpen] = useState(false);
  const [editingItem, setEditingItem] =
    useState<AdminAiTarotReadingTypeItem | null>(null);
  const [deletingItem, setDeletingItem] =
    useState<AdminAiTarotReadingTypeItem | null>(null);

  const openCreateForm = () => {
    setEditingItem(null);
    setFormOpen(true);
  };

  const openEditForm = (item: AdminAiTarotReadingTypeItem) => {
    setEditingItem(item);
    setFormOpen(true);
  };

  const closeForm = () => {
    setFormOpen(false);
    setEditingItem(null);
  };

  const handleConfirmDelete = () => {
    if (!deletingItem) {
      return;
    }

    deleteMutation.mutate(deletingItem.id, {
      onSuccess: () => {
        message.success(t("page.admin.aiTarotSettings.readingTypes.deleteSuccess"));
        setDeletingItem(null);
      },
      onError: (error) => message.error(getErrorMessage(error)),
    });
  };

  return (
    <div>
      <Flex
        justify="space-between"
        align="flex-start"
        wrap
        gap={token.marginMD}
        style={{ marginBottom: token.marginMD }}
      >
        <Text type="secondary" style={{ maxWidth: 640 }}>
          {t("page.admin.aiTarotSettings.readingTypes.description")}
        </Text>

        <Button type="primary" icon={<PlusOutlined />} onClick={openCreateForm}>
          {t("page.admin.aiTarotSettings.readingTypes.addButton")}
        </Button>
      </Flex>

      <ReadingTypeTable
        items={items}
        onEdit={openEditForm}
        onDelete={(item) => setDeletingItem(item)}
      />

      <ReadingTypeFormModal
        open={formOpen}
        item={editingItem}
        onClose={closeForm}
      />

      <DeleteAiTarotSettingModal
        open={deletingItem !== null}
        title={t("page.admin.aiTarotSettings.readingTypes.delete.title")}
        description={t(
          "page.admin.aiTarotSettings.readingTypes.delete.description",
          { code: deletingItem?.code ?? "" },
        )}
        loading={deleteMutation.isPending}
        onClose={() => setDeletingItem(null)}
        onConfirm={handleConfirmDelete}
      />
    </div>
  );
}
