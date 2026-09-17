import { convertISOToDate } from "@/utils";
import { Button, Card, Flex, theme } from "antd";
import { useTranslation } from "react-i18next";
import {
  DeleteOutlined,
  CalendarOutlined,
  ClockCircleOutlined,
} from "@ant-design/icons";
import { TarotCard } from "@/components";
import Text from "antd/es/typography/Text";
import type { CardData, GetAllReadingItem } from "@/types";
import type { ModalType } from "../HistoryPage";

interface TarotCardItemProps {
  data: GetAllReadingItem;
  setModalType: (type: ModalType) => void;
  setSelectedCard: (card: CardData | null) => void;
  setDeleteId: (id: string | null) => void;
}

// Alpha channel (hex) applied to the card's drop-shadow color.
const CARD_SHADOW_ALPHA = "26";

export default function TarotCardItem({
  data,
  setModalType,
  setSelectedCard,
  setDeleteId,
}: TarotCardItemProps) {
  const { token } = theme.useToken();
  const { t, i18n } = useTranslation();

  const { date, time } = convertISOToDate(data.createdAt, i18n.language);
  const cardName = t(`tarot.meaning.${data.cardCode}.name`);
  const orientation = data.isReversed
    ? t("tarot.position.reversed")
    : t("tarot.position.upright");

  const handleView = () => {
    setModalType("view");
    setSelectedCard({
      cardCode: data.cardCode,
      isReversed: data.isReversed,
    });
  };

  return (
    <Card
      hoverable
      role="button"
      tabIndex={0}
      onClick={handleView}
      onKeyDown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          handleView();
        }
      }}
      title={cardName + " · " + orientation}
      style={{
        borderRadius: token.borderRadiusLG,
        overflow: "hidden",
        border: `1px solid ${token.colorBorderSecondary}`,
        backdropFilter: "blur(10px)",
        boxShadow: token.boxShadowTertiary,
        cursor: "pointer",
        transition: "all 0.3s cubic-bezier(0.4, 0, 0.2, 1)",
      }}
      styles={{
        body: {
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          padding: `${token.paddingLG}px ${token.paddingMD}px ${token.paddingMD}px`,
        },
      }}
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
            setModalType("delete");
            setDeleteId(data.id);
          }}
        />
      }
    >
      {/* Tarot Card Preview */}
      <div
        style={{
          marginBottom: token.marginMD,
          transition: "transform 0.3s ease",
          filter: `drop-shadow(0 4px 12px ${token.colorPrimary}${CARD_SHADOW_ALPHA})`,
        }}
      >
        <TarotCard
          cardCode={data.cardCode}
          isUpright={!data.isReversed}
          size="md"
        />
      </div>
      {/* Date & Time Footer */}
      <Flex
        gap="small"
        vertical
        style={{
          width: "100%",
          borderTop: `1px solid ${token.colorBorderSecondary}`,
          paddingTop: token.paddingSM,
        }}
      >
        <Flex align="center" justify="center" gap={6}>
          <CalendarOutlined style={{ fontSize: 12 }} />
          <Text>{date}</Text>
          <ClockCircleOutlined style={{ fontSize: 12, marginLeft: 6 }} />
          <Text>{time}</Text>
        </Flex>
      </Flex>
    </Card>
  );
}
