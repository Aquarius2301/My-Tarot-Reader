import { Empty, Typography, theme, App, Spin } from "antd";
import { useTranslation } from "react-i18next";
import { useState } from "react";
import { useDeleteHistory, useGetAllReading } from "@/hooks/api";
import type { CardData, GetAllReadingItem } from "@/types";
import { getErrorMessage } from "@/utils";
import {
  DeleteReadingModal,
  TarotCardItem,
  TarotCardModal,
} from "./components";

const { Title, Text } = Typography;

export type ModalType = "view" | "delete" | null;

export default function HistoryPage() {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const { message } = App.useApp();

  const { data, isLoading } = useGetAllReading();
  const { mutate, isPending } = useDeleteHistory();

  const [modalType, setModalType] = useState<ModalType>(null);
  const [selectedCard, setSelectedCard] = useState<CardData | null>(null);

  const [deleteId, setDeleteId] = useState<string | null>(null);

  const closeViewModal = () => {
    setModalType(null);
    setSelectedCard(null);
  };

  const closeDeleteModal = () => {
    setModalType(null);
    setDeleteId(null);
  };

  const handleDelete = () => {
    if (!deleteId) return;

    mutate(deleteId, {
      onSuccess: () => {
        message.success(t("page.history.deleteSuccess"));
        closeDeleteModal();
      },
      onError: (err) => {
        message.error(getErrorMessage(err));
        closeDeleteModal();
      },
    });
  };

  if (isLoading) {
    return <Spin fullscreen />;
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
        <Title level={2} style={{ margin: 0 }}>
          {t("page.history.title")}
        </Title>
        <Text type="secondary">{t("page.history.subtitle")}</Text>
      </div>

      {(!data || data.items.length === 0) && (
        <Empty
          description={t("page.history.empty")}
          style={{ margin: `${token.marginXXL}px 0` }}
        />
      )}

      {data && data.items.length > 0 && (
        <>
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fill, minmax(220px, 1fr))",
              gap: token.marginLG,
            }}
          >
            {data.items.map((item: GetAllReadingItem) => (
              <TarotCardItem
                key={item.id}
                data={item}
                setModalType={setModalType}
                setSelectedCard={setSelectedCard}
                setDeleteId={setDeleteId}
              />
            ))}
          </div>

          <TarotCardModal
            open={modalType === "view"}
            selectedCard={selectedCard}
            onClose={closeViewModal}
          />

          <DeleteReadingModal
            open={modalType === "delete" && !!deleteId}
            loading={isPending}
            onClose={closeDeleteModal}
            onConfirm={handleDelete}
          />
        </>
      )}
    </div>
  );
}
