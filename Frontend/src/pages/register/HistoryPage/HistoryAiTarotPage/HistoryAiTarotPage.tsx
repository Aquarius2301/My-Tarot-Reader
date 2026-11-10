import { AppTitle } from "@/components";
import { useTranslation } from "react-i18next";
import { HistoryAiList } from "./HistoryAiList/HistoryAiList";

/** Lists all of the user's AI tarot readings; clicking one opens the full result. */
export default function HistoryAiTarotPage() {
  const { t } = useTranslation();

  return (
    <div>
      <AppTitle
        title={t("page.historyAiTarot.title")}
        subtitle={t("page.historyAiTarot.subtitle")}
      />

      <HistoryAiList />
    </div>
  );
}
