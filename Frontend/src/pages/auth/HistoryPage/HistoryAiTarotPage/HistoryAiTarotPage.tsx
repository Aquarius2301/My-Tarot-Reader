import { ErrorComponent, TarotCard } from "@/components";
import { AI_TAROT_CARD_COUNT_BY_VALUE } from "@/constants";
import { useDeleteAiTarotReading, useGetAllAiTarotReadings } from "@/hooks/api";
import { WEB_URL } from "@/routes";
import { convertISOToDate, getErrorMessage } from "@/utils";
import type { GetAllAiTarotReadingItem } from "@/types";
import {
  CalendarOutlined,
  ClockCircleOutlined,
  DeleteOutlined,
  ThunderboltOutlined,
} from "@ant-design/icons";
import {
  App,
  Button,
  Card,
  Empty,
  Flex,
  Spin,
  Tag,
  Typography,
  theme,
} from "antd";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { DeleteAiTarotModal } from "./components";

const { Title, Text, Paragraph } = Typography;

/** Lists all of the user's AI tarot readings; clicking one opens the full result. */
export default function HistoryAiTarotPage() {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const { message } = App.useApp();

  const { data, isLoading, refetch } = useGetAllAiTarotReadings();
  const { mutate, isPending } = useDeleteAiTarotReading();

  const [deleteId, setDeleteId] = useState<string | null>(null);

  const closeDeleteModal = () => setDeleteId(null);

  const handleDelete = () => {
    if (!deleteId) return;

    mutate(deleteId, {
      onSuccess: () => {
        message.success(t("page.historyAiTarot.deleteSuccess"));
        closeDeleteModal();
      },
      onError: (err) => {
        message.error(getErrorMessage(err));
        closeDeleteModal();
      },
    });
  };

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (!data) {
    return <ErrorComponent type="server" onRetry={refetch} />;
  }

  return (
    <div
      style={{
        maxWidth: 1080,
        margin: "0 auto",
        padding: `${token.paddingLG}px ${token.paddingMD}px`,
      }}
    >
      <div style={{ textAlign: "center", marginBottom: token.marginXL }}>
        <Title level={2} style={{ margin: 0 }}>
          {t("page.historyAiTarot.title")}
        </Title>
        <Text type="secondary">{t("page.historyAiTarot.subtitle")}</Text>
      </div>

      {data.items.length === 0 && (
        <Empty
          description={t("page.historyAiTarot.empty")}
          style={{ margin: `${token.marginXXL}px 0` }}
        />
      )}

      <Flex vertical gap={token.marginLG}>
        {data.items.map((item) => (
          <HistoryAiTarotItem
            key={item.id}
            item={item}
            onDelete={() => setDeleteId(item.id)}
          />
        ))}
      </Flex>

      <DeleteAiTarotModal
        open={!!deleteId}
        loading={isPending}
        onClose={closeDeleteModal}
        onConfirm={handleDelete}
      />
    </div>
  );
}

function HistoryAiTarotItem({
  item,
  onDelete,
}: {
  item: GetAllAiTarotReadingItem;
  onDelete: () => void;
}) {
  const { t, i18n } = useTranslation();
  const { token } = theme.useToken();
  const navigate = useNavigate();

  const { date, time } = convertISOToDate(item.createdAt, i18n.language);
  const typeLabel = t(`page.aiTarot.questionTypes.${item.type}`);
  const cardCount = AI_TAROT_CARD_COUNT_BY_VALUE[item.cardCount];

  const openReading = () => navigate(`${WEB_URL.aiTarotResult}/${item.id}`);

  return (
    <Card
      hoverable
      role="button"
      tabIndex={0}
      onClick={openReading}
      onKeyDown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          openReading();
        }
      }}
      style={{
        borderRadius: token.borderRadiusLG,
        overflow: "hidden",
        border: `1px solid ${token.colorBorderSecondary}`,
        boxShadow: token.boxShadowTertiary,
        cursor: "pointer",
        transition: "all 0.3s cubic-bezier(0.4, 0, 0.2, 1)",
      }}
      styles={{
        body: {
          display: "flex",
          flexDirection: "column",
          gap: token.marginSM,
          padding: token.paddingLG,
        },
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
            onDelete();
          }}
        />
      }
      title={
        <Flex align="center" gap={token.marginXS}>
          <ThunderboltOutlined style={{ color: token.colorPrimary }} />
          <Text strong ellipsis>{item.title}</Text>
        </Flex>
      }
    >
      {/* Drawn cards in a horizontal scroll strip */}
      <div
        style={{
          display: "flex",
          gap: token.paddingSM,
          overflowX: "auto",
          paddingBottom: token.paddingXS,
        }}
      >
        {item.cards.map((card, index) => (
          <div
            key={`${item.id}-${card.cardCode}-${index}`}
            style={{ flex: "0 0 auto" }}
          >
            <TarotCard
              cardCode={card.cardCode}
              isUpright={!card.isReversed}
              size="sm"
            />
          </div>
        ))}
      </div>

      {/* Date & category meta */}
      <Flex wrap align="center" gap={token.marginSM}>
        <Tag color="processing" style={{ marginInlineEnd: 0 }}>
          {typeLabel}
        </Tag>
        <Tag style={{ marginInlineEnd: 0 }}>
          {t("page.aiTarot.cardCountOption", { count: cardCount })}
        </Tag>
        <Flex align="center" gap={6}>
          <CalendarOutlined style={{ fontSize: 12 }} />
          <Text type="secondary">{date}</Text>
          <ClockCircleOutlined style={{ fontSize: 12, marginLeft: 6 }} />
          <Text type="secondary">{time}</Text>
        </Flex>
      </Flex>

      <Paragraph
        type="secondary"
        ellipsis={{ rows: 3 }}
        style={{ margin: 0 }}
      >
        {item.answerSummary}
      </Paragraph>
    </Card>
  );
}