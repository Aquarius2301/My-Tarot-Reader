import { useState } from "react";
import { App, Button, Flex, Typography, theme } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useDeleteAiTarotReadingCount } from "@/hooks/api";
import type { AdminAiTarotReadingCountItem } from "@/types";
import { getErrorMessage } from "@/utils";
import {
  DeleteAiTarotSettingModal,
  ReadingCountFormModal,
  ReadingCountTable,
} from "./components";

const { Text } = Typography;

export interface ReadingCountsTabProps {
  items: AdminAiTarotReadingCountItem[];
}

/** Spread configuration tab: lists, creates, edits and deletes reading counts. */
export default function ReadingCountsTab({ items }: ReadingCountsTabProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const { message } = App.useApp();
  const deleteMutation = useDeleteAiTarotReadingCount();

  const [formOpen, setFormOpen] = useState(false);
  const [editingItem, setEditingItem] =
    useState<AdminAiTarotReadingCountItem | null>(null);
  const [deletingItem, setDeletingItem] =
    useState<AdminAiTarotReadingCountItem | null>(null);

  const openCreateForm = () => {
    setEditingItem(null);
    setFormOpen(true);
  };

  const openEditForm = (item: AdminAiTarotReadingCountItem) => {
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
        message.success(t("page.admin.aiTarotSettings.readingCounts.deleteSuccess"));
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
          {t("page.admin.aiTarotSettings.readingCounts.description")}
        </Text>

        <Button type="primary" icon={<PlusOutlined />} onClick={openCreateForm}>
          {t("page.admin.aiTarotSettings.readingCounts.addButton")}
        </Button>
      </Flex>

      <ReadingCountTable
        items={items}
        onEdit={openEditForm}
        onDelete={(item) => setDeletingItem(item)}
      />

      <ReadingCountFormModal
        open={formOpen}
        item={editingItem}
        onClose={closeForm}
      />

      <DeleteAiTarotSettingModal
        open={deletingItem !== null}
        title={t("page.admin.aiTarotSettings.readingCounts.delete.title")}
        description={t(
          "page.admin.aiTarotSettings.readingCounts.delete.description",
          { cardCount: deletingItem?.cardCount ?? "" },
        )}
        loading={deleteMutation.isPending}
        onClose={() => setDeletingItem(null)}
        onConfirm={handleConfirmDelete}
      />
    </div>
  );
}
