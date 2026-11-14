import { AppTitle, ErrorView } from "@/components";
import type { AiDeepTarotTopic } from "@/constants";
import { useGetAiDeepTarotReadingById } from "@/hooks/api";
import { WEB_URL } from "@/routes";
import { Button, Flex, theme } from "antd";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router-dom";
import { AiDeepTarotResultItem } from "./components";

const DRAW_URL_BY_TOPIC: Record<AiDeepTarotTopic, string> = {
  twelveHouses: WEB_URL.aiDeepTarotTwelveHouses,
  twelveMonths: WEB_URL.aiDeepTarotTwelveMonths,
  crossroads: WEB_URL.aiDeepTarotCrossroads,
};

/** Shows a created deep tarot reading, whichever spread it belongs to. */
export default function AiDeepTarotResultPage() {
  const { readingId } = useParams<{ readingId: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const { data, isLoading, isError, refetch } = useGetAiDeepTarotReadingById(
    readingId ?? "",
    !!readingId,
  );

  if (isError) {
    return <ErrorView type="server" onRetry={refetch} />;
  }

  if (!isLoading && !data) {
    return <ErrorView type="server" onRetry={refetch} />;
  }

  return (
    <div>
      <AppTitle
        title={data?.title ?? t("page.aiDeepTarot.result.title")}
        isLoading={isLoading}
      />

      <AiDeepTarotResultItem data={data} isLoading={isLoading} />

      {data && (
        <Flex justify="center" style={{ marginTop: token.marginXL }}>
          <Button
            type="primary"
            size="large"
            onClick={() => navigate(DRAW_URL_BY_TOPIC[data.topic])}
          >
            {t("page.aiDeepTarot.result.drawAgain")}
          </Button>
        </Flex>
      )}
    </div>
  );
}
