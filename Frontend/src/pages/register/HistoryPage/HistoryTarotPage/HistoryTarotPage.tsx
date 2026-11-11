import { AppTitle, EmptyView, ErrorView, PaginationBar } from "@/components";
import { useTranslation } from "react-i18next";
import { HistoryList } from "./HistoryList";
import { App } from "antd";
import { useState } from "react";
import { useDeleteHistory, useGetAllReading } from "@/hooks/api";
import { useNavigate } from "react-router-dom";
import { WEB_URL } from "@/routes";
import type { CardData } from "@/types";
import { TarotCardModal } from "./TarotCardModal";
import { DeleteReadingModal } from "./DeleteReadingModal";
import { getErrorMessage } from "@/utils";

export const PAGE_SIZE = 12; // Choose 12 per page because it fits nicely for 1 - 4 columns

export type ModalType = "view" | "delete" | null;

export default function HistoryTarotPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { message } = App.useApp();

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState<number>(PAGE_SIZE);

  const [modalType, setModalType] = useState<ModalType>(null);
  const [selectedCard, setSelectedCard] = useState<CardData | null>(null);
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const { data, isLoading, refetch, isError } = useGetAllReading({
    page,
    pageSize,
  });

  const { mutate, isPending } = useDeleteHistory();

  const handleDelete = () => {
    if (!deleteId) return;

    mutate(deleteId, {
      onSuccess: () => {
        if (data?.items.length === 1 && page > 1) {
          setPage((p) => p - 1);
        }
        message.success(t("page.history.deleteSuccess"));
        closeModal();
      },
      onError: (err) => {
        message.error(getErrorMessage(err));
        closeModal();
      },
    });
  };

  const closeModal = () => {
    setModalType(null);
    setSelectedCard(null);
    setDeleteId(null);
  };

  const openView = (card: CardData) => {
    setSelectedCard(card);
    setModalType("view");
  };

  const openDelete = (id: string) => {
    setDeleteId(id);
    setModalType("delete");
  };

  if (isError) {
    return <ErrorView type="server" onRetry={refetch} />;
  }

  if (!isLoading && data?.totalItems === 0) {
    return (
      <EmptyView
        description={t("page.history.empty")}
        button={{
          children: t("page.history.emptyButton"),
          onClick: () => navigate(WEB_URL.tarot),
        }}
      />
    );
  }

  return (
    <div>
      {/* Header Section */}
      <AppTitle title={t("page.history.subtitle")} isLoading={isLoading} />

      <HistoryList
        skeletonCount={PAGE_SIZE}
        reading={data?.items || []}
        isLoading={isLoading}
        onView={openView}
        onDelete={openDelete}
      />

      {data && (
        <PaginationBar
          total={data.totalItems}
          page={page}
          pageSize={pageSize}
          onChange={(nextPage, nextPageSize) => {
            setPage(nextPage);
            setPageSize(nextPageSize);
          }}
        />
      )}

      {/* Modal */}
      <TarotCardModal
        open={modalType === "view"}
        selectedCard={selectedCard}
        onClose={closeModal}
      />

      <DeleteReadingModal
        open={modalType === "delete" && !!deleteId}
        loading={isPending}
        onClose={closeModal}
        onConfirm={handleDelete}
      />
    </div>
  );
}
{
  /* {data.totalItems === 0 && (
        <Empty
          description={t("page.history.empty")}
          style={{ margin: `${token.marginXXL}px 0` }}
        />
      )} */
}
