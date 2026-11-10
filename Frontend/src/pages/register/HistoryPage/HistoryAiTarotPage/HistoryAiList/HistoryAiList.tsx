import { ErrorView, PaginationBar } from "@/components";
import { App, Empty, Flex } from "antd";
import { DeleteAiTarotModal } from "./DeleteAiTarotModal";
import { useDeleteAiTarotReading, useGetAllAiTarotReadings } from "@/hooks/api";
import { useState } from "react";
import { PAGINATION_PAGE_SIZE } from "@/constants";
import { getErrorMessage } from "@/utils";
import { useTranslation } from "react-i18next";
import { HistoryAiTarotItem } from "./HistoryAiTarotItem";
import { HistoryAiTarotItemLoading } from "./HistoryAiTarotItemLoading";

export function HistoryAiList() {
  const { t } = useTranslation();
  const { message } = App.useApp();

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState<number>(PAGINATION_PAGE_SIZE);

  const { data, isLoading, refetch } = useGetAllAiTarotReadings({
    page,
    pageSize,
  });

  const { mutate, isPending } = useDeleteAiTarotReading();

  const [deleteId, setDeleteId] = useState<string | null>(null);

  const closeDeleteModal = () => setDeleteId(null);

  const handleDelete = () => {
    if (!deleteId) return;

    mutate(deleteId, {
      onSuccess: () => {
        if (data?.items.length === 1 && page > 1) {
          setPage((p) => p - 1);
        }
        message.success(t("page.historyAiTarot.deleteSuccess"));
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
        <Flex vertical gap={16}>
          {Array.from({ length: PAGINATION_PAGE_SIZE }).map((_, index) => (
            <HistoryAiTarotItemLoading key={index} />
          ))}
        </Flex>
      );
    }

    return <ErrorView type="server" onRetry={refetch} />;
  }

  return (
    <>
      {data.totalItems === 0 && (
        <Empty description={t("page.historyAiTarot.empty")} />
      )}
      <Flex
        vertical
        gap={16}
        // style={{
        //   opacity: isPlaceholderData ? 0.6 : 1,
        //   transition: "opacity 0.2s",
        // }}
      >
        {data.items.map((item) => (
          <HistoryAiTarotItem
            key={item.id}
            item={item}
            onDelete={() => setDeleteId(item.id)}
          />
        ))}
      </Flex>
      <PaginationBar
        total={data.totalItems}
        page={page}
        pageSize={pageSize}
        onChange={(nextPage, nextPageSize) => {
          setPage(nextPage);
          setPageSize(nextPageSize);
        }}
      />
      <DeleteAiTarotModal
        open={!!deleteId}
        loading={isPending}
        onClose={closeDeleteModal}
        onConfirm={handleDelete}
      />
    </>
  );
}
