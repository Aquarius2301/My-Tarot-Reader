import { Card, Flex, Tag, Typography, theme } from "antd";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { AiMarkdown, DrawnCardRow, ResponsiveModal } from "@/components";
import type { AIReadHistoryResult } from "@/types";
import { parseAiHistoryCards } from "@/utils";

const { Title } = Typography;

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
    () =>
      parseAiHistoryCards(history?.cards ?? "").map(({ code, isReversed }) => ({
        cardCode: code,
        isReversed,
      })),
    [history?.cards],
  );

  const orientationTagLabel = (isReversed: boolean) =>
    isReversed ? t("page.history.reversed") : t("page.history.upright");

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
          <DrawnCardRow
            cards={cards}
            size="md"
            withOrientationTag
            orientationTagLabel={orientationTagLabel}
          />

          {/* Full AI interpretation */}
          <Title level={5} style={{ marginTop: 0 }}>
            {t("page.history.aiReadingInterpretation")}
          </Title>
          <Card style={{ textAlign: "left" }}>
            <AiMarkdown content={history.answer} />
          </Card>
        </>
      )}
    </ResponsiveModal>
  );
}