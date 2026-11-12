import { AppTitle, EmptyView, ErrorView, PaginationBar } from "@/components";
import { useTranslation } from "react-i18next";
import { HistoryAiDeepList } from "./HistoryAiDeepList";
import { useState } from "react";
import { PAGINATION_PAGE_SIZE } from "@/constants";
import {
  useDeleteAiDeepTarotReading,
  useGetAllAiDeepTarotReadings,
} from "@/hooks/api";
import { App } from "antd";
import { getErrorMessage } from "@/utils";
import { DeleteAiDeepTarotModal } from "./DeleteAiDeepTarotModal";
import { useNavigate } from "react-router-dom";
import { WEB_URL } from "@/routes";

/** Lists all of the user's deep tarot readings; clicking one opens the full result. */
export default function HistoryAiDeepTarotPage() {
  const { t } = useTranslation();

  const { message } = App.useApp();
  const navigate = useNavigate();

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState<number>(PAGINATION_PAGE_SIZE);

  const { data, isLoading, refetch, isError } = useGetAllAiDeepTarotReadings({
    page,
    pageSize,
  });

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

  if (isError) {
    return <ErrorView type="server" onRetry={refetch} />;
  }

  if (data?.totalItems === 0) {
    return (
      <EmptyView
        description={t("page.historyAiDeepTarot.empty")}
        button={{
          children: t("page.historyAiDeepTarot.emptyButton"),
          onClick: () => navigate(WEB_URL.aiDeepTarotTwelveHouses),
        }}
      />
    );
  }

  return (
    <div>
      <AppTitle
        title={t("page.historyAiDeepTarot.subtitle")}
        isLoading={isLoading}
      />

      <HistoryAiDeepList
        reading={data?.items ?? []}
        onDelete={(id) => setDeleteId(id)}
        isLoading={isLoading}
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
      <DeleteAiDeepTarotModal
        open={!!deleteId}
        loading={isPending}
        onClose={closeDeleteModal}
        onConfirm={handleDelete}
      />
    </div>
  );
}
