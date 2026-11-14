import { Flex } from "antd";
import { HistoryAiTarotItem } from "./HistoryAiTarotItem";
import type { GetAllAiTarotReadingItem } from "@/types";
import { PAGINATION_PAGE_SIZE } from "@/constants";

export interface HistoryAiListProps {
  reading: GetAllAiTarotReadingItem[];
  onDelete: (id: string) => void;
  onView: (id: string) => void;
  isLoading?: boolean;
}

export function HistoryAiList({
  reading,
  onDelete,
  onView,
  isLoading = false,
}: HistoryAiListProps) {
  if (isLoading) {
    return (
      <Flex vertical gap={16}>
        {Array.from({ length: PAGINATION_PAGE_SIZE }).map((_, key) => (
          <HistoryAiTarotItem key={key} isLoading />
        ))}
      </Flex>
    );
  }

  return (
    <Flex vertical gap={16}>
      {reading.map((item) => (
        <HistoryAiTarotItem
          key={item.id}
          item={item}
          onDelete={() => onDelete(item.id)}
          onView={() => onView(item.id)}
        />
      ))}
    </Flex>
  );
}
