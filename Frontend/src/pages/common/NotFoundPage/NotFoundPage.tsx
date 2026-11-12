import { WEB_URL } from "@/routes";
import { Button, Result } from "antd";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";

export default function NotFoundPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  return (
    <div
      style={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        minHeight: "100%",
        padding: "40px 16px",
      }}
    >
      <Result
        status="404"
        title={t("page.notFound.title")}
        subTitle={t("page.notFound.description")}
        extra={[
          <Button
            key="home"
            type="primary"
            onClick={() => navigate(WEB_URL.home)}
          >
            {t("page.notFound.backHome")}
          </Button>,
          <Button key="back" onClick={() => navigate(-1)}>
            {t("page.notFound.goBack")}
          </Button>,
        ]}
      />
    </div>
  );
}
