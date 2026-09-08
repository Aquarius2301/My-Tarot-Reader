import {
  Button,
  Card,
  Flex,
  Popconfirm,
  Tag,
  Typography,
  theme,
  type CardProps,
} from "antd";
import {
  CalendarOutlined,
  ClockCircleOutlined,
  DeleteOutlined,
} from "@ant-design/icons";
import { useMemo, type KeyboardEvent } from "react";
import ReactMarkdown from "react-markdown";
import { useTranslation } from "react-i18next";
import { TarotCard } from "@/components";
import type { AIReadHistoryResult } from "@/types";
import { convertISOToDate, parseAiHistoryCards } from "@/utils";

const { Text, Paragraph, Title } = Typography;

export interface AIHistoryCardProps {
  /** The AI read-history record to render. */
  history: AIReadHistoryResult;
  /** Opens the full-reading modal for this record. */
  onView: () => void;
  /** Callback invoked after the user confirms deletion of this record. */
  onDelete: (id: string) => void;
}

/** Renders a single AI read-history item as a full-width row: cards + answer with header metadata. */
export default function AIHistoryCard({
  history,
  onView,
  onDelete,
}: AIHistoryCardProps) {
  const { t, i18n } = useTranslation();
  const { token } = theme.useToken();

  const cards = useMemo(
    () => parseAiHistoryCards(history.cards),
    [history.cards],
  );
  // const visibleCards = cards.slice(0, AI_HISTORY_MAX_VISIBLE_CARDS);
  const { date, time } = convertISOToDate(history.createdAt, i18n.language);

  const cardLabel = t(`select.cardCount.${history.cardCount}`);
  const questionTypeLabel = t(`select.questionType.${history.questionType}`);
  const viewLabel = t("page.history.aiViewAria", { reading: cardLabel });

  // The whole row is the "view reading" control; make it keyboard-operable.
  const handleKeyDown = (e: KeyboardEvent<HTMLDivElement>) => {
    if (e.key === "Enter" || e.key === " ") {
      e.preventDefault();
      onView();
    }
  };

  const cardProps: CardProps = {
    hoverable: true,
    role: "button",
    tabIndex: 0,
    "aria-label": viewLabel,
    onClick: onView,
    onKeyDown: handleKeyDown,
  };

  return (
    <Card
      {...cardProps}
      title={
        <Flex gap={6} align="center" wrap="wrap">
          <Tag
            color="geekblue"
            style={{
              borderRadius: token.borderRadiusSM,
              border: "none",
              marginInlineEnd: 0,
            }}
          >
            {questionTypeLabel}
          </Tag>
          <Tag
            style={{
              borderRadius: token.borderRadiusSM,
              border: "none",
              marginInlineEnd: 0,
            }}
          >
            {cardLabel}
          </Tag>
        </Flex>
      }
      extra={
        <Popconfirm
          title={t("page.history.aiDeleteConfirmTitle")}
          okText={t("page.history.aiDeleteOk")}
          okButtonProps={{ danger: true }}
          cancelText={t("page.history.aiDeleteCancel")}
          onConfirm={() => onDelete(history.id)}
        >
          <Button
            type="text"
            danger
            icon={<DeleteOutlined />}
            aria-label={t("page.history.aiDeleteAria", { reading: cardLabel })}
            onClick={(e) => e.stopPropagation()}
          />
        </Popconfirm>
      }
      style={{
        width: "100%",
        borderRadius: token.borderRadiusLG,
        overflow: "hidden",
        border: `1px solid ${token.colorBorderSecondary}`,
        backdropFilter: "blur(10px)",
        boxShadow: token.boxShadowTertiary,
        cursor: "pointer",
        transition: "all 0.3s cubic-bezier(0.4, 0, 0.2, 1)",
      }}
      styles={{ body: { padding: token.paddingLG } }}
    >
      {/* Card faces (visual summary, not interactive). Always kept on a single
      horizontally-scrollable line so they never wrap/drop — a scrollbar
      appears whenever the row overflows the available width. */}
      <Flex
        gap={token.marginXS}
        align="center"
        wrap="nowrap"
        style={{
          marginBottom: token.marginMD,
          overflowX: "auto",
          overflowY: "hidden",
          paddingBottom: 4,
        }}
      >
        {cards.map((card, index) => (
          <div key={index} style={{ flex: "0 0 auto" }}>
            <TarotCard
              cardCode={card.code}
              isUpright={!card.isReversed}
              size="sm"
            />
          </div>
        ))}
      </Flex>

      {/* Truncated AI interpretation (rendered markdown, clamped) */}
      <div
        style={{
          maxHeight: 132,
          overflow: "hidden",
          maskImage:
            "linear-gradient(to bottom, black calc(100% - 16px), transparent)",
          WebkitMaskImage:
            "linear-gradient(to bottom, black calc(100% - 16px), transparent)",
        }}
      >
        <ReactMarkdown
          components={{
            h1: ({ children }) => <Title level={5}>{children}</Title>,
            h2: ({ children }) => <Title level={5}>{children}</Title>,
            h3: ({ children }) => <Title level={5}>{children}</Title>,
            h4: ({ children }) => <Title level={5}>{children}</Title>,
            p: ({ children }) => (
              <Paragraph style={{ marginBottom: 8, marginTop: 0 }}>
                {children}
              </Paragraph>
            ),
            strong: ({ children }) => <Text strong>{children}</Text>,
            li: ({ children }) => (
              <li style={{ marginBottom: 2 }}>{children}</li>
            ),
          }}
        >
          {history.answer}
        </ReactMarkdown>
      </div>

      {/* Date & time footer */}
      <Flex
        align="center"
        justify="center"
        gap={6}
        style={{
          borderTop: `1px solid ${token.colorBorderSecondary}`,
          paddingTop: token.paddingSM,
          marginTop: token.marginMD,
          width: "100%",
        }}
      >
        <CalendarOutlined style={{ fontSize: 12 }} />
        <Text type="secondary" style={{ fontSize: 12 }}>
          {date}
        </Text>
        <ClockCircleOutlined style={{ fontSize: 12, marginLeft: 6 }} />
        <Text type="secondary" style={{ fontSize: 12 }}>
          {time}
        </Text>
      </Flex>
    </Card>
  );
}
