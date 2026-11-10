import { ErrorView, PaginationBar } from "@/components";
import { App, Empty, Flex, theme } from "antd";
import { DeleteAiDeepTarotModal } from "./DeleteAiDeepTarotModal";
import { getErrorMessage } from "@/utils";
import {
  useDeleteAiDeepTarotReading,
  useGetAllAiDeepTarotReadings,
} from "@/hooks/api";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { PAGINATION_PAGE_SIZE } from "@/constants";
import { HistoryAiDeepTarotItem } from "./HistoryAiDeepTarotItem";
import { HistoryAiDeepTarotItemLoading } from "./HistoryAiDeepTarotItemLoading";

export function HistoryAiDeepList() {
  const { token } = theme.useToken();
  const { message } = App.useApp();
  const { t } = useTranslation();

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState<number>(PAGINATION_PAGE_SIZE);

  const { data, isLoading, isPlaceholderData, refetch } =
    useGetAllAiDeepTarotReadings({ page, pageSize });
  const { mutate, isPending } = useDeleteAiDeepTarotReading();

  const [deleteId, setDeleteId] = useState<string | null>(null);

  const closeDeleteModal = () => setDeleteId(null);

  const handleDelete = () => {
    if (!deleteId) return;

    mutate(deleteId, {
      onSuccess: () => {
        if (data?.items.length === 1 && page > 1) {
          setPage((p) => p - 1);
        }
        message.success(t("page.historyAiDeepTarot.deleteSuccess"));
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
        <Flex vertical gap={token.marginLG}>
          {Array.from({ length: PAGINATION_PAGE_SIZE }).map((_, index) => (
            <HistoryAiDeepTarotItemLoading key={index} />
          ))}
        </Flex>
      );
    }

    return <ErrorView type="server" onRetry={refetch} />;
  }

  return (
    <>
      {data.totalItems === 0 && (
        <Empty
          description={t("page.historyAiDeepTarot.empty")}
          style={{ margin: `${token.marginXXL}px 0` }}
        />
      )}
      <Flex
        vertical
        gap={token.marginLG}
        style={{
          opacity: isPlaceholderData ? 0.6 : 1,
          transition: "opacity 0.2s",
        }}
      >
        {data.items.map((item) => (
          <HistoryAiDeepTarotItem
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
      <DeleteAiDeepTarotModal
        open={!!deleteId}
        loading={isPending}
        onClose={closeDeleteModal}
        onConfirm={handleDelete}
      />
    </>
  );
}
