import { TarotCard } from "@/components";
import {
  AI_DEEP_TAROT_TOPIC_LABEL_KEYS,
  type AiDeepTarotTopic,
} from "@/constants";
import { WEB_URL } from "@/routes";
import type { GetAllAiDeepTarotReadingItem } from "@/types";
import { convertISOToDate } from "@/utils";
import { Button, Card, Divider, Flex, Tag, theme } from "antd";
import Text from "antd/es/typography/Text";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import {
  CalendarOutlined,
  ClockCircleOutlined,
  DeleteOutlined,
  HomeOutlined,
  BranchesOutlined,
} from "@ant-design/icons";
import Paragraph from "antd/es/typography/Paragraph";
import type { ReactNode } from "react";
import { HistoryAiDeepTarotItemLoading } from "./HistoryAiDeepTarotItemLoading";

/** Badge icon in front of each reading, so the spread is recognisable. */
const TOPIC_ICONS = {
  twelveHouses: <HomeOutlined />,
  twelveMonths: <CalendarOutlined />,
  crossroads: <BranchesOutlined />,
} as const satisfies Record<AiDeepTarotTopic, ReactNode>;

export interface HistoryAiDeepTarotItemProps {
  item?: GetAllAiDeepTarotReadingItem;
  onDelete?: () => void;
  isLoading?: boolean;
}

export function HistoryAiDeepTarotItem({
  item,
  onDelete,
}: HistoryAiDeepTarotItemProps) {
  const { t, i18n } = useTranslation();
  const { token } = theme.useToken();
  const navigate = useNavigate();

  if (!item) {
    return <HistoryAiDeepTarotItemLoading />;
  }

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
            onDelete?.();
          }}
        />
      }
      title={
        <Flex align="center" gap={token.marginXS}>
          <span style={{ color: token.colorPrimary }}>
            {TOPIC_ICONS[item.topic]}
          </span>
          <Text strong ellipsis>
            {item.title} (
            {t("page.aiTarot.cardCountOption", {
              count: item.cards.length,
            })}
            )
          </Text>
        </Flex>
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

        {/* Date & topic meta */}
        <Divider size="small" />
        <Flex wrap align="center" gap={16}>
          <Tag>{topicLabel}</Tag>

          <Flex align="center" gap={8}>
            <Text type="secondary">
              <CalendarOutlined /> {date}
            </Text>

            <Text type="secondary">
              <ClockCircleOutlined /> {time}
            </Text>
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
