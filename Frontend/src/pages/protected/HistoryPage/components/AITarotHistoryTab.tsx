import { App, Empty, Flex, Spin, theme } from "antd";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import {
  useDeleteAiHistory,
  useGetAiHistoryReadings,
} from "@/hooks/api";
import { getErrorMessage } from "@/utils";
import type { AIReadHistoryResult } from "@/types";
import AIHistoryCard from "./AIHistoryCard";
import AIReadingModal from "./AIReadingModal";

/** Tab content that shows the AI-powered tarot read-history list. */
export default function AITarotHistoryTab() {
  const { t } = useTranslation();
  const { data, isLoading } = useGetAiHistoryReadings();
  const { mutate: deleteAiHistory } = useDeleteAiHistory();
  const { token } = theme.useToken();
  const { message } = App.useApp();
  const [selected, setSelected] = useState<AIReadHistoryResult | null>(null);

  const handleDelete = (id: string) => {
    deleteAiHistory(id, {
      onSuccess: () => message.success(t("page.history.aiDeleteSuccess")),
      onError: (err) => message.error(getErrorMessage(err)),
    });
  };

  if (isLoading) {
    return (
      <Flex justify="center" style={{ padding: `${token.marginXXL}px 0` }}>
        <Spin />
      </Flex>
    );
  }

  return (
    <Flex vertical gap={token.marginLG}>
      {(!data || data.length === 0) && (
        <Empty
          description={t("page.history.aiEmpty")}
          style={{ margin: `${token.marginXXL}px 0` }}
        />
      )}

      {data?.map((item: AIReadHistoryResult) => (
        <AIHistoryCard
          key={item.id}
          history={item}
          onView={() => setSelected(item)}
          onDelete={handleDelete}
        />
      ))}

      {/* Full-reading modal */}
      <AIReadingModal
        open={!!selected}
        history={selected}
        onClose={() => setSelected(null)}
      />
    </Flex>
  );
}