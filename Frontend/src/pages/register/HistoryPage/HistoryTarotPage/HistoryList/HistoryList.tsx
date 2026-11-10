import { useState } from "react";
import { App, Col, Empty, Row, theme } from "antd";
import { useTranslation } from "react-i18next";
import { ErrorView, PaginationBar } from "@/components";
import { useDeleteHistory, useGetAllReading } from "@/hooks/api";
import type { CardData } from "@/types";
import { getErrorMessage } from "@/utils";
import { TarotCardItem } from "./TarotCardItem";
import { TarotCardItemLoading } from "./TarotCardItemLoading";
import { TarotCardModal } from "./TarotCardModal";
import { DeleteReadingModal } from "./DeleteReadingModal";

type ModalType = "view" | "delete" | null;

const PAGE_SIZE = 12; // Choose 12 per page because it fits nicely for 1 - 4 columns

export function HistoryList() {
  const { token } = theme.useToken();
  const { t } = useTranslation();
  const { message } = App.useApp();

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState<number>(PAGE_SIZE);

  const { data, isLoading, refetch } = useGetAllReading({
    page,
    pageSize,
  });
  const { mutate, isPending } = useDeleteHistory();

  const [modalType, setModalType] = useState<ModalType>(null);
  const [selectedCard, setSelectedCard] = useState<CardData | null>(null);
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const openViewModal = (card: CardData) => {
    setSelectedCard(card);
    setModalType("view");
  };

  const openDeleteModal = (id: string) => {
    setDeleteId(id);
    setModalType("delete");
  };

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
        if (data?.items.length === 1 && page > 1) {
          setPage((p) => p - 1);
        }
        message.success(t("page.history.deleteSuccess"));
        closeDeleteModal();
      },
      onError: (err) => {
        message.error(getErrorMessage(err));
        closeDeleteModal();
      },
    });
  };

  if (!data) {
    if (isLoading) {
      return (
        <Row gutter={[16, 16]}>
          {Array.from({ length: PAGE_SIZE }).map((_, index) => (
            <Col xs={24} sm={12} md={8} lg={6} key={index}>
              <TarotCardItemLoading />
            </Col>
          ))}
        </Row>
      );
    }

    return <ErrorView type="server" onRetry={refetch} />;
  }

  return (
    <>
      {data.totalItems === 0 && (
        <Empty
          description={t("page.history.empty")}
          style={{ margin: `${token.marginXXL}px 0` }}
        />
      )}

      {data.totalItems > 0 && (
        <>
          <Row gutter={[16, 16]}>
            {data.items.map((item) => (
              <Col xs={24} sm={12} md={8} lg={6} key={item.id}>
                <TarotCardItem
                  key={item.id}
                  data={item}
                  onView={openViewModal}
                  onDelete={openDeleteModal}
                />
              </Col>
            ))}
          </Row>

          <PaginationBar
            total={data.totalItems}
            page={page}
            pageSize={pageSize}
            onChange={(nextPage, nextPageSize) => {
              setPage(nextPage);
              setPageSize(nextPageSize);
            }}
          />

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
    </>
  );
}
