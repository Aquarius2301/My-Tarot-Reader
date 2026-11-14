import { AppTitle, ErrorView } from "@/components";
import { useGetAiTarotReadingById } from "@/hooks/api";
import { WEB_URL } from "@/routes";
import { Button, Flex, theme } from "antd";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router-dom";
import { AiTarotResultItem } from "./components";

/** Shows a created AI tarot reading: title, drawn cards and its meanings. */
export default function AiTarotResultPage() {
  const { readingId } = useParams<{ readingId: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const { data, isLoading, isError, refetch } = useGetAiTarotReadingById(
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
        title={data?.title ?? t("page.aiTarot.result.title")}
        isLoading={isLoading}
      />

      <AiTarotResultItem data={data} isLoading={isLoading} />

      <Flex justify="center" style={{ marginTop: token.marginXL }}>
        <Button
          type="primary"
          size="large"
          onClick={() => navigate(WEB_URL.aiTarot)}
        >
          {t("page.aiTarot.result.drawAgain")}
        </Button>
      </Flex>
    </div>
  );
}
