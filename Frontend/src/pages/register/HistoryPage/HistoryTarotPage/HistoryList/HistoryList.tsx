import { Col, Row } from "antd";
import type { CardData, GetAllReadingItem } from "@/types";
import { TarotCardItem } from "./TarotCardItem/TarotCardItem";

export interface HistoryListProps {
  reading: GetAllReadingItem[];
  isLoading?: boolean;
  onView: (card: CardData) => void;
  onDelete: (id: string) => void;
  skeletonCount: number;
}

export function HistoryList({
  reading,
  isLoading = false,
  onView,
  onDelete,
  skeletonCount,
}: HistoryListProps) {
  if (isLoading) {
    return (
      <Row gutter={[16, 16]}>
        {Array.from({ length: skeletonCount }).map((_, index) => (
          <Col xs={24} sm={12} md={8} lg={6} key={index}>
            <TarotCardItem />
          </Col>
        ))}
      </Row>
    );
  }

  return (
    <Row gutter={[16, 16]}>
      {reading.map((item) => (
        <Col xs={24} sm={12} md={8} lg={6}>
          <TarotCardItem
            key={item.id}
            data={item ?? {}}
            onView={onView}
            onDelete={onDelete}
          />
        </Col>
      ))}
    </Row>
  );
}
