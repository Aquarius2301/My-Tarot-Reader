import { Flex } from "antd";
import type { GetAllAiDeepTarotReadingItem } from "@/types";
import { PAGINATION_PAGE_SIZE } from "@/constants";
import { HistoryAiDeepTarotItem } from "./HistoryAiDeepTarotItem";

export interface HistoryAiDeepListProps {
  reading: GetAllAiDeepTarotReadingItem[];
  onDelete: (id: string) => void;
  isLoading?: boolean;
}

export function HistoryAiDeepList({
  reading,
  onDelete,
  isLoading = false,
}: HistoryAiDeepListProps) {
  if (isLoading) {
    return (
      <Flex vertical gap={16}>
        {Array.from({ length: PAGINATION_PAGE_SIZE }).map((_, key) => (
          <HistoryAiDeepTarotItem key={key} isLoading />
        ))}
      </Flex>
    );
  }

  return (
    <Flex vertical gap={16}>
      {reading.map((item) => (
        <HistoryAiDeepTarotItem
          key={item.id}
          item={item}
          onDelete={() => onDelete(item.id)}
        />
      ))}
    </Flex>
  );
}
