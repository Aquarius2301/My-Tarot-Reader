import { Card, Flex, Tag, Typography, theme } from "antd";
import { useMemo } from "react";
import ReactMarkdown from "react-markdown";
import { useTranslation } from "react-i18next";
import { ResponsiveModal, TarotCard } from "@/components";
import type { AIReadHistoryResult } from "@/types";
import { parseAiHistoryCards } from "@/utils";

const { Title, Paragraph, Text } = Typography;

/** Modal that shows the full contents of a single AI read-history record. */
export default function AIReadingModal({
  open,
  history,
  onClose,
}: {
  open: boolean;
  history: AIReadHistoryResult | null;
  onClose: () => void;
}) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const cards = useMemo(
    () => parseAiHistoryCards(history?.cards ?? ""),
    [history?.cards],
  );

  return (
    <ResponsiveModal
      open={open && !!history}
      onClose={onClose}
      title={history ? `${t("page.history.aiReadingTitle")} · ${t(`select.cardCount.${history.cardCount}`)}` : ""}
    >
      {history && (
        <>
          <Flex align="center" gap="small" style={{ marginBottom: token.marginMD }}>
            <Tag color="geekblue" style={{ borderRadius: token.borderRadiusSM, border: "none" }}>
              {t(`select.questionType.${history.questionType}`)}
            </Tag>
            <Tag style={{ borderRadius: token.borderRadiusSM, border: "none" }}>
              {t(`select.cardCount.${history.cardCount}`)}
            </Tag>
          </Flex>

          {/* All drawn cards */}
          <Flex gap={token.marginSM} wrap="wrap" justify="center" style={{ marginBottom: token.marginLG }}>
            {cards.map((card, index) => (
              <Flex key={`${card.code}-${index}`} vertical align="center" gap={4}>
                <TarotCard cardCode={card.code} isUpright={!card.isReversed} size="md" />
                <Tag
                  color={card.isReversed ? "volcano" : "purple"}
                  style={{ borderRadius: token.borderRadiusSM, border: "none" }}
                >
                  {card.isReversed
                    ? t("page.history.reversed")
                    : t("page.history.upright")}
                </Tag>
              </Flex>
            ))}
          </Flex>

          {/* Full AI interpretation */}
          <Title level={5} style={{ marginTop: 0 }}>
            {t("page.history.aiReadingInterpretation")}
          </Title>
          <Card style={{ textAlign: "left" }}>
            <ReactMarkdown
              components={{
                h1: ({ children }) => <Title level={3}>{children}</Title>,
                h2: ({ children }) => <Title level={4}>{children}</Title>,
                h3: ({ children }) => <Title level={5}>{children}</Title>,
                h4: ({ children }) => <Title level={5}>{children}</Title>,
                p: ({ children }) => (
                  <Paragraph
                    style={{ marginBottom: 12, fontSize: 16, lineHeight: 1.7 }}
                  >
                    {children}
                  </Paragraph>
                ),
                strong: ({ children }) => <Text strong>{children}</Text>,
                li: ({ children }) => <li style={{ marginBottom: 4 }}>{children}</li>,
              }}
            >
              {history.answer}
            </ReactMarkdown>
          </Card>
        </>
      )}
    </ResponsiveModal>
  );
}