import { AppTitle, EmptyView, ErrorView, PaginationBar } from "@/components";
import { useTranslation } from "react-i18next";
import { HistoryAiList } from "./HistoryAiList";
import { useState } from "react";
import { PAGINATION_PAGE_SIZE } from "@/constants";
import { useDeleteAiTarotReading, useGetAllAiTarotReadings } from "@/hooks/api";
import { App } from "antd";
import { getErrorMessage } from "@/utils";
import { DeleteAiTarotModal } from "./DeleteAiTarotModal";
import { useNavigate } from "react-router-dom";
import { WEB_URL } from "@/routes";

/** Lists all of the user's AI tarot readings; clicking one opens the full result. */
export default function HistoryAiTarotPage() {
  const { t } = useTranslation();

  const { message } = App.useApp();
  const navigate = useNavigate();

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState<number>(PAGINATION_PAGE_SIZE);

  const { data, isLoading, refetch, isError } = useGetAllAiTarotReadings({
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

  const handleView = (id: string) => navigate(`${WEB_URL.aiTarotResult}/${id}`);

  if (isError) {
    return <ErrorView type="server" onRetry={refetch} />;
  }

  if (data?.totalItems === 0) {
    return (
      <EmptyView
        description={t("page.historyAiTarot.empty")}
        button={{
          children: t("page.historyAiTarot.emptyButton"),
          onClick: () => navigate(WEB_URL.aiTarot),
        }}
      />
    );
  }

  return (
    <div>
      <AppTitle
        title={t("page.historyAiTarot.subtitle")}
        isLoading={isLoading}
      />

      <HistoryAiList
        reading={data?.items ?? []}
        onDelete={(id) => setDeleteId(id)}
        onView={(id) => handleView(id)}
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
      <DeleteAiTarotModal
        open={!!deleteId}
        loading={isPending}
        onClose={closeDeleteModal}
        onConfirm={handleDelete}
      />
    </div>
  );
}
