import { ErrorComponent, TarotCard } from "@/components";
import {
  AI_DEEP_TAROT_TOPIC_LABEL_KEYS,
  type AiDeepTarotTopic,
} from "@/constants";
import {
  useDeleteAiDeepTarotReading,
  useGetAllAiDeepTarotReadings,
} from "@/hooks/api";
import { WEB_URL } from "@/routes";
import { convertISOToDate, getErrorMessage } from "@/utils";
import type { GetAllAiDeepTarotReadingItem } from "@/types";
import {
  BranchesOutlined,
  CalendarOutlined,
  ClockCircleOutlined,
  DeleteOutlined,
  HomeOutlined,
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
import { useState, type ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { DeleteAiDeepTarotModal } from "./components";

/** Badge icon in front of each reading, so the spread is recognisable. */
const TOPIC_ICONS = {
  twelveHouses: <HomeOutlined />,
  twelveMonths: <CalendarOutlined />,
  crossroads: <BranchesOutlined />,
} as const satisfies Record<AiDeepTarotTopic, ReactNode>;

const { Title, Text, Paragraph } = Typography;

/** Lists all of the user's deep tarot readings; clicking one opens the full result. */
export default function HistoryAiDeepTarotPage() {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const { message } = App.useApp();

  const { data, isLoading, refetch } = useGetAllAiDeepTarotReadings();
  const { mutate, isPending } = useDeleteAiDeepTarotReading();

  const [deleteId, setDeleteId] = useState<string | null>(null);

  const closeDeleteModal = () => setDeleteId(null);

  const handleDelete = () => {
    if (!deleteId) return;

    mutate(deleteId, {
      onSuccess: () => {
        message.success(t("page.historyAiDeepTarot.deleteSuccess"));
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
          {t("page.historyAiDeepTarot.title")}
        </Title>
        <Text type="secondary">{t("page.historyAiDeepTarot.subtitle")}</Text>
      </div>

      {data.items.length === 0 && (
        <Empty
          description={t("page.historyAiDeepTarot.empty")}
          style={{ margin: `${token.marginXXL}px 0` }}
        />
      )}

      <Flex vertical gap={token.marginLG}>
        {data.items.map((item) => (
          <HistoryAiDeepTarotItem
            key={item.id}
            item={item}
            onDelete={() => setDeleteId(item.id)}
          />
        ))}
      </Flex>

      <DeleteAiDeepTarotModal
        open={!!deleteId}
        loading={isPending}
        onClose={closeDeleteModal}
        onConfirm={handleDelete}
      />
    </div>
  );
}

function HistoryAiDeepTarotItem({
  item,
  onDelete,
}: {
  item: GetAllAiDeepTarotReadingItem;
  onDelete: () => void;
}) {
  const { t, i18n } = useTranslation();
  const { token } = theme.useToken();
  const navigate = useNavigate();

  const { date, time } = convertISOToDate(item.createdAt, i18n.language);
  const topicLabel = t(AI_DEEP_TAROT_TOPIC_LABEL_KEYS[item.topic]);

  const openReading = () => navigate(`${WEB_URL.aiDeepTarotResult}/${item.id}`);

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
          aria-label={t("page.historyAiDeepTarot.deleteConfirm")}
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
          <span style={{ color: token.colorPrimary }}>
            {TOPIC_ICONS[item.topic]}
          </span>
          <Text strong ellipsis>
            {item.title}
          </Text>
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

      {/* Date & topic meta */}
      <Flex wrap align="center" gap={token.marginSM}>
        <Tag color="processing" style={{ marginInlineEnd: 0 }}>
          {topicLabel}
        </Tag>
        <Flex align="center" gap={6}>
          <CalendarOutlined style={{ fontSize: 12 }} />
          <Text type="secondary">{date}</Text>
          <ClockCircleOutlined style={{ fontSize: 12, marginLeft: 6 }} />
          <Text type="secondary">{time}</Text>
        </Flex>
      </Flex>

      {item.question && (
        <Paragraph
          type="secondary"
          ellipsis={{ rows: 2 }}
          style={{ margin: 0, fontStyle: "italic" }}
        >
          {item.question}
        </Paragraph>
      )}

      <Paragraph type="secondary" ellipsis={{ rows: 3 }} style={{ margin: 0 }}>
        {item.answerSummary}
      </Paragraph>
    </Card>
  );
}
