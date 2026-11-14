import { Button, Card, Divider, Flex } from "antd";
import { useTranslation } from "react-i18next";
import {
  DeleteOutlined,
  CalendarOutlined,
  ClockCircleOutlined,
} from "@ant-design/icons";
import { TarotCard } from "@/components";
import Text from "antd/es/typography/Text";
import type { CardData, GetAllReadingItem } from "@/types";
import { convertISOToDate } from "@/utils";
import { TarotCardItemLoading } from "./TarotCardItemLoading";

interface TarotCardItemProps {
  data?: GetAllReadingItem;
  onView?: (card: CardData) => void;
  onDelete?: (id: string) => void;
}

export function TarotCardItem({ data, onView, onDelete }: TarotCardItemProps) {
  const { t, i18n } = useTranslation();

  if (!data) {
    return <TarotCardItemLoading />;
  }

  const { date, time } = convertISOToDate(data.createdAt, i18n.language);
  const cardName = t(`tarot.meaning.${data.cardCode}.name`);
  const orientation = data.isReversed
    ? t("tarot.position.reversed")
    : t("tarot.position.upright");

  const handleView = () => {
    onView?.({
      cardCode: data.cardCode,
      isReversed: data.isReversed,
    });
  };

  return (
    <Card
      hoverable
      onClick={handleView}
      title={cardName + " · " + orientation}
      style={{ height: "100%" }}
      extra={
        <Button
          type="text"
          danger
          icon={<DeleteOutlined />}
          aria-label={t("page.history.deleteConfirm")}
          onClick={(e) => {
            // Prevent the click from bubbling up to the Card's onClick,
            // which would otherwise also open the "view" modal.
            e.stopPropagation();
            onDelete?.(data.id);
          }}
        />
      }
    >
      {/* Tarot Card Preview */}
      <Flex justify="center">
        <TarotCard
          cardCode={data.cardCode}
          isUpright={!data.isReversed}
          size="md"
        />
      </Flex>

      {/* Date & Time Footer */}
      <Divider />

      <Flex
        gap="small"
        vertical
        style={{
          width: "100%",
        }}
      >
        <Flex wrap align="center" justify="space-between" gap={6}>
          <Text>
            <CalendarOutlined /> {date}
          </Text>
          <Text>
            <ClockCircleOutlined /> {time}
          </Text>
        </Flex>
      </Flex>
    </Card>
  );
}
