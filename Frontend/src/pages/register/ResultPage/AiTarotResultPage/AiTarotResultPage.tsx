import { AppTitle } from "@/components";
import { WEB_URL } from "@/routes";
import { Button, Flex, theme } from "antd";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { AiTarotResultItem } from "./AiTarotResultItem/AiTarotResultItem";

/** Shows a created AI tarot reading: title, drawn cards and its meanings. */
export default function AiTarotResultPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { token } = theme.useToken();

  return (
    <div>
      <AppTitle title={t("page.aiTarot.result.title")} />

      <AiTarotResultItem />

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
