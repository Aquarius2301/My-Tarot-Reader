import { TarotCard } from "@/components";
import type { GetAllAiTarotReadingItem } from "@/types";
import { convertISOToDate } from "@/utils";
import { Button, Card, Divider, Flex, Tag } from "antd";
import Text from "antd/es/typography/Text";
import { useTranslation } from "react-i18next";
import {
  CalendarOutlined,
  ClockCircleOutlined,
  DeleteOutlined,
} from "@ant-design/icons";
import Paragraph from "antd/es/typography/Paragraph";
import { HistoryAiTarotItemLoading } from "./HistoryAiTarotItemLoading";

export interface HistoryAiTarotItemProps {
  item?: GetAllAiTarotReadingItem;
  onDelete?: () => void;
  onView?: () => void;
  isLoading?: boolean;
}

export function HistoryAiTarotItem({
  item,
  onDelete,
  onView,
}: HistoryAiTarotItemProps) {
  const { t, i18n } = useTranslation();

  if (!item) {
    return <HistoryAiTarotItemLoading />;
  }

  const { date, time } = convertISOToDate(item.createdAt, i18n.language);
  const typeLabel = t(`page.aiTarot.questionTypes.${item.type}`);
  const cardCount = item.cardCount;

  return (
    <Card
      hoverable
      role="button"
      tabIndex={0}
      onClick={() => onView?.()}
      onKeyDown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          onView?.();
        }
      }}
      extra={
        <Button
          type="text"
          danger
          icon={<DeleteOutlined />}
          aria-label={t("page.historyAiTarot.deleteConfirm")}
          onClick={(e) => {
            // Prevent the click from bubbling up to the Card's onClick,
            // which would otherwise open the full reading page.
            e.stopPropagation();
            onDelete?.();
          }}
        />
      }
      title={
        <Text strong ellipsis>
          {item.title} (
          {t("page.aiTarot.cardCountOption", { count: cardCount })})
        </Text>
      }
    >
      <Flex vertical gap={16}>
        {/* Drawn cards in a horizontal scroll strip */}
        <Flex
          gap={8}
          style={{
            overflowX: "auto",
          }}
        >
          {item.cards.map((card, index) => (
            <TarotCard
              key={`${item.id}-${card.cardCode}-${index}`}
              cardCode={card.cardCode}
              isUpright={!card.isReversed}
              size="sm"
            />
          ))}
        </Flex>

        {/* Date & category meta */}
        <Divider size="small" />
        <Flex wrap align="center" gap={16}>
          <Tag>{typeLabel}</Tag>

          <Flex align="center" gap={8}>
            <Text type="secondary">
              <CalendarOutlined /> {date}
            </Text>

            <Text type="secondary">
              <ClockCircleOutlined /> {time}
            </Text>
          </Flex>
        </Flex>

        <Paragraph
          type="secondary"
          ellipsis={{ rows: 3 }}
          style={{ margin: 0 }}
        >
          {item.answerSummary}
        </Paragraph>
      </Flex>
    </Card>
  );
}
