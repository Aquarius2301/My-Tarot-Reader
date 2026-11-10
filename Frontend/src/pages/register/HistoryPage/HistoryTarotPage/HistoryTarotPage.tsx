import { AppTitle } from "@/components";
import { useTranslation } from "react-i18next";
import { HistoryList } from "./HistoryList";

export default function HistoryTarotPage() {
  const { t } = useTranslation();

  return (
    <div>
      {/* Header Section */}
      <AppTitle
        title={t("page.history.title")}
        subtitle={t("page.history.subtitle")}
      />

      <HistoryList />
    </div>
  );
}
